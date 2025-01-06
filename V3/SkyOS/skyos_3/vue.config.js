module.exports = {
	publicPath: process.env.ELECTRON_NODE_INTEGRATION ? '' : '/',
	pluginOptions: {
		electronBuilder: {
			builderOptions: {
				win: {
					icon: './src/sys/assets/icons/icon.ico'
				}
			}
		}
	}
}