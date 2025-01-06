
import _Vue from 'vue';
import { AxiosInstance, AxiosResponse, AxiosError } from 'axios';

interface AuthServiceResponse {
  authenticated: boolean;
  token?: string;
}

interface User {
    account_id: string;
    created_at: number;
    updated_at: number;
    alias: string;
    email: string;
    first_name: string;
    last_name: string;
    last_login_at: number;
    role: number;
    status: number;
  }

class Auth {

  public authenticated: boolean = null;
  public authToken = localStorage.getItem('at');
  public refreshToken = localStorage.getItem('rt');
  public currentAccount: User = null;
  private vue: _Vue = null;

  constructor(Vue: _Vue) {
    this.vue = Vue;
  }

  public login(emailIn: string, passIn: string): boolean {

    if (this.authToken) {
      this.onChange(true);
      return true;
    }

    (this.vue.$root.$apiService.axios as AxiosInstance).post('/auth/login', {
      username: emailIn,
      password: passIn,
    })
      .then((response: AxiosResponse) => {
        if (response.status === 200) {
          this.authToken = response.data.auth_token;
          this.refreshToken = response.data.refresh_token;
          localStorage.setItem('rt', response.data.refresh_token);
          localStorage.setItem('at', response.data.auth_token);
          this.vue.$apiService.updateToken(response.data.auth_token);
          this.onChange(true);
          return true;
        } else {
          this.onChange(false);
          return false;
        }
      })
      .catch((error: AxiosError) => {
        if (error.response) {
          switch (error.response.status) {
            case 400: {
              this.onChange(false);
              return false;
            }
            default: {
              return true;
            }
          }
        } else {
          return false;
        }
      });

    return false;
  }

  public loggedIn(): boolean {
    return this.authToken != null && this.refreshToken != null && this.authenticated;
  }

  public validateToken(callback: (status: boolean) => void) {

    this.vue.$root.$apiService.updateToken("Wildcard");
    this.onChange(true);
    callback(true);

    return;

    //setTimeout(() => {
    //  if (this.authToken != null && this.refreshToken != null) {
    //    this.onChange(true);
    //    cb(true);
    //  } else {
    //    this.onChange(false);
    //    cb(false);
    //  }
    //}, 400);

    this.vue.$root.$apiService.updateToken(this.authToken);

    this.vue.$root.$apiService.axios.get('/me', {})
      .then(response => {
        if (response.status === 200) {
          const apiUser = response.data;
          this.currentAccount = {
            account_id: apiUser.id,
            created_at: apiUser.created_at,
            updated_at: apiUser.updated_at,
            alias: apiUser.alias,
            email: apiUser.email,
            first_name: apiUser.first_name,
            last_name: apiUser.last_name,
            last_login_at: apiUser.last_login_at,
            role: apiUser.role,
            status: apiUser.status,
          }
          this.onChange(true);
          callback(true);
        } else {
          this.currentAccount = null;
          this.onChange(false);
          callback(false);
        }
      })
      .catch(error => {
        if (error.response) {
          switch (error.response.status) {
            default: {
              this.currentAccount = null;
              this.onChange(false);
              callback(false);
            }
          }
        } else {
          this.currentAccount = null;
          this.onChange(false);
          callback(false);
        }
      });



    //self.$root.$apiService.axios.post('/auth', {})
    //  .then(response => {
    //    if (response.status === 200) {
    //      this.authenticated = true;
    //      cb(true);
    //      return true;
    //    } else {
    //      this.authenticated = false;
    //      cb(false);
    //      return false;
    //    }
    //  })
    //  .catch(error => {
    //    if (error.response) {
    //      switch (error.response.status) {
    //        default: {
    //          this.authenticated = false;
    //          cb(false);
    //          return false;
    //        }
    //      }
    //    } else {
    //      this.authenticated = false;
    //      cb(false);
    //      return false;
    //    }
    //  });

  }

  private onChange(state: boolean) {
    if (state !== this.authenticated) {
      this.authenticated = state;
      if (this.authenticated) {
        this.vue.$root.$router.push({ path: window.location.pathname });
      } else {
        this.vue.$root.$router.push({ path: '/l' });
      }
    }
  }

}

export default {
  install: (Vue: typeof _Vue) => { //, options?: any
    let installed = false;
    Vue.mixin({
      beforeCreate() {
        if (!installed) {
          installed = true;
          Vue.prototype.$authService = new Auth(this);
        }
      }
    });
  }
};

declare module 'vue/types/vue' {
  interface Vue {
    $authService: Auth;
  }
}
