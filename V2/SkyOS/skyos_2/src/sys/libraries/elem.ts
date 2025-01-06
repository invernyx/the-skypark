export default class Eljs {

    public static getID(id: string) {
        return document.getElementById(id);
    }

    public static getSel(selector: string, parent: HTMLElement) {

        if (parent === undefined) {
            return document.querySelector(selector);
        } else {
            return parent.querySelector(selector);
        }
    }

    public static getSelAll(selector: string, parent: HTMLElement) {
        if (parent === undefined) {
            return document.querySelectorAll(selector);
        } else {
            return parent.querySelectorAll(selector);
        }
    }

    public static hasClass(element: HTMLElement, cl: string) {
        return element.classList.contains(cl);
    }

    public static addClass(element: Element, cl: string) {
        if (!element.classList.contains(cl)) {
            element.classList.add(cl);
        }
    }

    public static removeClass(element: HTMLElement, cl: string) {
        if (element.classList.contains(cl)) {
            element.classList.remove(cl);
        }
    }

    public static setAttr(element: HTMLElement, data: string, value: any) {
        element.setAttribute(data, value);
    }

    public static getAttr(element: HTMLElement, data: string) {
        return element.getAttribute(data);
    }

    public static removeAttr(element: HTMLElement, data: string) {
        element.removeAttribute(data);
    }

    public static getData(element: HTMLElement, data: string): any {
        return element.getAttribute('data-' + data);
    }

    public static setData(element: HTMLElement, data: string, value: any) {
        element.setAttribute('data-' + data, value);
    }

    public static removeData(element: HTMLElement, data: string) {
        element.removeAttribute('data-' + data);
    }

    public static on(element: HTMLElement, event: any, ret: any) {
        element.addEventListener(event, ret);
    }

    public static off(element: HTMLElement, event: any, ret: any) {
        element.removeEventListener(event, ret);
    }

    public static tap(elem: HTMLElement, ev: any, rangeCheck: number, stopPropagate: boolean, preventDefault: boolean) {

        let tapDown = false;
        let tapClickLoc = [0, 0];

        function tap_md(event: any) {
            //elephant("MD");
            if (stopPropagate) {
                event.stopPropagation();
            }
            tapDown = true;
            tapClickLoc = [event.pageX, event.pageY];
        }

        function tap_mu(event: any) {
            //elephant("MU");
            if (stopPropagate) {
                event.stopPropagation();
            }
            if (tapDown) {
                tapDown = false;
                if (Math.abs(tapClickLoc[0] - event.pageX) <= rangeCheck && Math.abs(tapClickLoc[1] - event.pageY) <= rangeCheck) {
                    ev(event);
                }
            }
        }

        function tap_ts(event: any) {
            if (preventDefault) {
                event.preventDefault();
            }
            if (stopPropagate) {
                event.stopPropagation();
            }
            tapDown = true;
            tapClickLoc = [event.touches[0].pageX, event.touches[0].pageY];
        }

        function tap_te(event: any) {
            if (preventDefault) {
                event.preventDefault();
            }
            if (stopPropagate) {
                event.stopPropagation();
            }
            if (tapDown) {
                tapDown = false;
                if (Math.abs(tapClickLoc[0] - event.changedTouches[0].pageX) <= rangeCheck && Math.abs(tapClickLoc[1] - event.changedTouches[0].pageY) <= rangeCheck) {
                    ev(event);
                }
            }
        }

        if (stopPropagate === undefined) {
            stopPropagate = true;
        }

        if (preventDefault === undefined) {
            preventDefault = true;
        }

        if (rangeCheck === undefined) {
            rangeCheck = 3;
        }

        if (('ontouchstart' in window)) {
            elem.addEventListener("touchstart", tap_ts);
            elem.addEventListener("touchend", tap_te);
        } else {
            elem.addEventListener("mousedown", tap_md);
            elem.addEventListener("mouseup", tap_mu);
        }
    }

    public static round(numb: number, dec: number) {
        const numToFixedDp = Number(numb).toFixed(dec);
        return Number(numToFixedDp);
    }

    public static getAvg(array: number[]) {
        const total = array.reduce((acc, c) => acc + c, 0);
        return total / array.length;
    }

    public static createEl(type: string, append?: HTMLElement, cl?: string) {

        if (append === undefined) {
            append = null;
        }

        if (cl === undefined) {
            cl = null;
        }

        const el = document.createElement(type);
        if (append != null) {
            append.append(el);
        }
        if (cl != null) {
            el.className = cl;
        }
        return el;
    }

    public static GetDistance(Lat1: number, Long1: number, Lat2: number, Long2: number, unit: "N" | "m") {

        let dDistance = 0;
        const dLat1InRad = Lat1 * (Math.PI / 180.0);
        const dLong1InRad = Long1 * (Math.PI / 180.0);
        const dLat2InRad = Lat2 * (Math.PI / 180.0);
        const dLong2InRad = Long2 * (Math.PI / 180.0);

        const dLongitude = dLong2InRad - dLong1InRad;
        const dLatitude = dLat2InRad - dLat1InRad;

        // Intermediate result a.
        const a = Math.pow(Math.sin(dLatitude / 2.0), 2.0) + Math.cos(dLat1InRad) * Math.cos(dLat2InRad) * Math.pow(Math.sin(dLongitude / 2.0), 2.0);

        // Intermediate result c (great circle distance in Radians).
        const c = 2.0 * Math.asin(Math.sqrt(a));

        // Distance.
        // var kEarthRadiusMiles = 3956.0;
        const KEarthRadiusKms = 6376.5;
        dDistance = KEarthRadiusKms * c;

        if (unit === "N") { dDistance = dDistance * 0.539957 }
        if (unit === "m") { dDistance = dDistance * 1000 }

        return dDistance;
    }

    public static getNumGUID(): number {
        let output = "";
        while (output.length < 8) {
            output += Math.floor(Math.random() * 9.9999).toString();
        }
        return Number(output);
    }

    public static genGUID() {
        /* tslint:disable */
        return 'xxxxxx'.replace(/[xy]/g, function(c) {
          const r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
          return v.toString(16);
        });
        /* tslint:enable */
	}

	public static limiter(min :number, max :number, value :number){
		if(value > max){
			return max;
		} else if(value < min){
			return min;
		} else {
			return value;
		}
	}

    public static normalizeAngle180(angle: number) {
        let newAngle = angle;
        while (newAngle <= -180) { newAngle += 360 }
        while (newAngle > 180) { newAngle -= 360 }
        return newAngle;
    }

    public static normalizeAngle360(angle: number) {
        let newAngle = angle;
        while (newAngle <= 0) { newAngle += 360 }
        while (newAngle > 360) { newAngle -= 360 }
        return newAngle;
	}

	public static BearingThresholdHitTest(previousNumber :number, newNumber :number, threshold :number) {
		const prevCourseNormal = Eljs.normalizeAngle360(previousNumber);
		const newCourseNormal = Eljs.normalizeAngle360(newNumber);

		const prevCourseBearingOffset = Eljs.MapCompareBearings(prevCourseNormal, threshold);
		const newCourseBearingOffset = Eljs.MapCompareBearings(newCourseNormal, threshold);

		//console.log(previousNumber);
		//console.log(prevCourseBearingOffset + ' vs ' + newCourseBearingOffset + ' on ' + threshold);

		if(prevCourseBearingOffset != 0) {
			if(prevCourseBearingOffset < 0 && newCourseBearingOffset > 0 || prevCourseBearingOffset > 0 && newCourseBearingOffset < 0) {
				if(Math.abs(prevCourseBearingOffset - newCourseBearingOffset) < 180) {
					return true;
				}
			}
		}

		return false;
	}

    public static GetBearing(startLat: number, startLng: number, destLat: number, destLng: number) {
        startLat = this.toRadians(startLat);
        startLng = this.toRadians(startLng);
        destLat = this.toRadians(destLat);
        destLng = this.toRadians(destLng);

        const y = Math.sin(destLng - startLng) * Math.cos(destLat);
        const x = Math.cos(startLat) * Math.sin(destLat) - Math.sin(startLat) * Math.cos(destLat) * Math.cos(destLng - startLng);
        let brng = Math.atan2(y, x);
        brng = this.toDegrees(brng);
        return (brng + 360) % 360;
	}

	public static MapCompareBearings(initial :number, final :number)
	{
		if (initial > 360 || initial < 0 || final > 360 || final < 0)
		{
			//throw some error
		}

		const diff = final - initial;
		const absDiff = Math.abs(diff);

		if (absDiff <= 180)
		{
			return absDiff == 180 ? absDiff : diff;
		}

		else if (final > initial)
		{
			return absDiff - 360;
		}

		else
		{
			return 360 - absDiff;
		}
	}

	public static MapOffsetPosition(lon :number, lat :number, distanceMeters :number, heading :number)
        {
            const R = 6378.14; //Earth Radious in KM
            const distanceKM = distanceMeters / 1000;

            const latitude1 = lat * (Math.PI / 180);
            const longitude1 = lon * (Math.PI / 180);
            const brng = heading * (Math.PI / 180);

            let latitude2 = Math.asin(Math.sin(latitude1) * Math.cos(distanceKM / R) + Math.cos(latitude1) * Math.sin(distanceKM / R) * Math.cos(brng));
            let longitude2 = longitude1 + Math.atan2(Math.sin(brng) * Math.sin(distanceKM / R) * Math.cos(latitude1), Math.cos(distanceKM / R) - Math.sin(latitude1) * Math.sin(latitude2));

            latitude2 = latitude2 * (180 / Math.PI);
            longitude2 = longitude2 * (180 / Math.PI);

            return [longitude2, latitude2];
	}

	public static DistPointToPoly($startX, $startY, $endX, $endY, $pointX, $pointY) {

		// list($distanceSegment, $x, $y) = point_to_line_segment_distance($startX,$startY, $endX,$endY, $pointX,$pointY);

		// Adapted from Philip Nicoletti's function, found here: http://www.codeguru.com/forum/printthread.php?t=194400

		const $r_numerator = ($pointX - $startX) * ($endX - $startX) + ($pointY - $startY) * ($endY - $startY);
		const $r_denominator = ($endX - $startX) * ($endX - $startX) + ($endY - $startY) * ($endY - $startY);
		const $r = $r_numerator / $r_denominator;


		const $px = $startX + $r * ($endX - $startX);
		const $py = $startY + $r * ($endY - $startY);

		const $s = (($startY-$pointY) * ($endX - $startX) - ($startX - $pointX) * ($endY - $startY) ) / $r_denominator;

		const $distanceLine = Math.abs($s) * Math.sqrt($r_denominator);

		let $closest_point_on_segment_X = $px;
		let $closest_point_on_segment_Y = $py;

		let $distanceSegment = 0;
		if ( ($r >= 0) && ($r <= 1) ) {
			$distanceSegment = $distanceLine;
		} else {
			const $dist1 = ($pointX - $startX) * ($pointX - $startX) + ($pointY - $startY) * ($pointY - $startY);
			const $dist2 = ($pointX - $endX) * ($pointX - $endX) + ($pointY - $endY) * ($pointY - $endY);
			if ($dist1 < $dist2) {
				$closest_point_on_segment_X = $startX;
				$closest_point_on_segment_Y = $startY;
				$distanceSegment = Math.sqrt($dist1);
			}
			else {
				$closest_point_on_segment_X = $endX;
				$closest_point_on_segment_Y = $endY;
				$distanceSegment = Math.sqrt($dist2);
			}
		}

		return [$distanceSegment, $closest_point_on_segment_X, $closest_point_on_segment_Y, $r];
	}

    public static toRadians(degrees: number) {
        return degrees * Math.PI / 180;
    }

    public static toDegrees(radians: number) {
        return radians * 180 / Math.PI;
	}

	public static CalculateRank(xp :number, gain :number) {

		// https://gamedev.stackexchange.com/questions/13638/algorithm-for-dynamically-calculating-a-level-based-on-experience-points
		const XP = xp;
		const Level = Math.floor((Math.sqrt(100 * ((2 * XP) + 25)) + 50) / 100);
		const LevelNext = Level + 1;
		const XPStart = (Math.pow(Level, 2) + Level) / 2 * 100 - (Level * 100);
		const XPEnd = (Math.pow(LevelNext, 2) + LevelNext) / 2 * 100 - (LevelNext * 100);
		const XPRange = (XPEnd - XPStart);
		const XPDelta = XP - XPStart;
		const LevelProgress = (1 / XPRange) * (XP - XPStart);
		const LevelGain = (1 / XPRange) * gain;

		return {
			level: Level,
			progress: LevelProgress,
			range: XPRange,
			floor: XPStart,
			ceil: XPEnd,
			delta: XPDelta,
			gainProgress: LevelGain,
			levelUp: LevelProgress + LevelGain > 1
		}
	}

	public static RGBToHSL(r :number, g :number, b :number) {

		// Make r, g, and b fractions of 1
		r /= 255;
		g /= 255;
		b /= 255;

		// Find greatest and smallest channel values
		const cmin = Math.min(r,g,b);
		const cmax = Math.max(r,g,b);
		const delta = cmax - cmin;
		let h = 0;
		let s = 0;
		let l = 0;

		// Calculate hue
		// No difference
		if (delta == 0)
			h = 0;
		// Red is max
		else if (cmax == r)
			h = ((g - b) / delta) % 6;
		// Green is max
		else if (cmax == g)
			h = (b - r) / delta + 2;
		// Blue is max
		else
			h = (r - g) / delta + 4;

		h = Math.round(h * 60);

		// Make negative hues positive behind 360°
		if (h < 0)
			h += 360;

		// Calculate lightness
		l = (cmax + cmin) / 2;

		// Calculate saturation
		s = delta == 0 ? 0 : delta / (1 - Math.abs(2 * l - 1));

		// Multiply l and s by 100
		s = +(s * 100);
		l = +(l * 100);

		return {
			h: h,
			s: s,
			l: l
		};
	}

	public static HSLToRGB(h :number, s :number, l :number) {
		// Must be fractions of 1
		s /= 100;
		l /= 100;

		const c = (1 - Math.abs(2 * l - 1)) * s;
		const x = c * (1 - Math.abs((h / 60) % 2 - 1));
		const m = l - c/2;

		let r = 0;
		let g = 0;
		let b = 0;

		if (0 <= h && h < 60) {
			r = c; g = x; b = 0;
			} else if (60 <= h && h < 120) {
			r = x; g = c; b = 0;
			} else if (120 <= h && h < 180) {
			r = 0; g = c; b = x;
			} else if (180 <= h && h < 240) {
			r = 0; g = x; b = c;
			} else if (240 <= h && h < 300) {
			r = x; g = 0; b = c;
			} else if (300 <= h && h < 360) {
			r = c; g = 0; b = x;
			}
			r = Math.round((r + m) * 255);
			g = Math.round((g + m) * 255);
			b = Math.round((b + m) * 255);

		return {
			r: r,
			g: g,
			b: b
		}
	}

    public static isDark(r: number,g: number,b: number) {
        return this.getBrightness(r,g,b) < 150;
    }

    public static getBrightness(r: number,g: number,b: number) {
        return (r * 299 + g * 587 + b * 114) / 1000;
	}

	public static getDOMParents(el :Node) {
		let currentEl = el;
		const path = [] as Node[];
		path.push(el);
		while(currentEl.parentNode) {
			currentEl = currentEl.parentNode;
			path.push(currentEl);
		}
		return path;
	}

	public static Easings = {

    	//x :number, t :number, b :number, c :number, d :number

		//t: current value,
		//b: min,
		//c: max-min,
		//d: max

		//t: current time,
		//b: begining value,
		//c: change in value,
		//d: duration

		easeInQuad: function (x :number, t :number, b :number, c :number, d :number) {
			return c*(t/=d)*t + b;
		},
		easeOutQuad: function (x :number, t :number, b :number, c :number, d :number) {
			return -c *(t/=d)*(t-2) + b;
		},
		easeInOutQuad: function (x :number, t :number, b :number, c :number, d :number) {
			if ((t/=d/2) < 1) return c/2*t*t + b;
			return -c/2 * ((--t)*(t-2) - 1) + b;
		},
		easeInCubic: function (x :number, t :number, b :number, c :number, d :number) {
			return c*(t/=d)*t*t + b;
		},
		easeOutCubic: function (x :number, t :number, b :number, c :number, d :number) {
			return c*((t=t/d-1)*t*t + 1) + b;
		},
		easeInOutCubic: function (x :number, t :number, b :number, c :number, d :number) {
			if ((t/=d/2) < 1) return c/2*t*t*t + b;
			return c/2*((t-=2)*t*t + 2) + b;
		},
		easeInQuart: function (x :number, t :number, b :number, c :number, d :number) {
			return c*(t/=d)*t*t*t + b;
		},
		easeOutQuart: function (x :number, t :number, b :number, c :number, d :number) {
			return -c * ((t=t/d-1)*t*t*t - 1) + b;
		},
		easeInOutQuart: function (x :number, t :number, b :number, c :number, d :number) {
			if ((t/=d/2) < 1) return c/2*t*t*t*t + b;
			return -c/2 * ((t-=2)*t*t*t - 2) + b;
		},
		easeInQuint: function (x :number, t :number, b :number, c :number, d :number) {
			return c*(t/=d)*t*t*t*t + b;
		},
		easeOutQuint: function (x :number, t :number, b :number, c :number, d :number) {
			return c*((t=t/d-1)*t*t*t*t + 1) + b;
		},
		easeInOutQuint: function (x :number, t :number, b :number, c :number, d :number) {
			if ((t/=d/2) < 1) return c/2*t*t*t*t*t + b;
			return c/2*((t-=2)*t*t*t*t + 2) + b;
		},
		easeInSine: function (x :number, t :number, b :number, c :number, d :number) {
			return -c * Math.cos(t/d * (Math.PI/2)) + c + b;
		},
		easeOutSine: function (x :number, t :number, b :number, c :number, d :number) {
			return c * Math.sin(t/d * (Math.PI/2)) + b;
		},
		easeInOutSine: function (x :number, t :number, b :number, c :number, d :number) {
			return -c/2 * (Math.cos(Math.PI*t/d) - 1) + b;
		},
		easeInExpo: function (x :number, t :number, b :number, c :number, d :number) {
			return (t==0) ? b : c * Math.pow(2, 10 * (t/d - 1)) + b;
		},
		easeInExpoEx: function (x :number, t :number, b :number, c :number, d :number) {
			return (t==0) ? b : c * Math.pow(5, 10 * (t/d - 1)) + b;
		},
		easeOutExpo: function (x :number, t :number, b :number, c :number, d :number) {
			return (t==d) ? b+c : c * (-Math.pow(2, -10 * t/d) + 1) + b;
		},
		easeInOutExpo: function (x :number, t :number, b :number, c :number, d :number) {
			if (t==0) return b;
			if (t==d) return b+c;
			if ((t/=d/2) < 1) return c/2 * Math.pow(2, 10 * (t - 1)) + b;
			return c/2 * (-Math.pow(2, -10 * --t) + 2) + b;
		},
		easeInCirc: function (x :number, t :number, b :number, c :number, d :number) {
			return -c * (Math.sqrt(1 - (t/=d)*t) - 1) + b;
		},
		easeOutCirc: function (x :number, t :number, b :number, c :number, d :number) {
			return c * Math.sqrt(1 - (t=t/d-1)*t) + b;
		},
		easeInOutCirc: function (x :number, t :number, b :number, c :number, d :number) {
			if ((t/=d/2) < 1) return -c/2 * (Math.sqrt(1 - t*t) - 1) + b;
			return c/2 * (Math.sqrt(1 - (t-=2)*t) + 1) + b;
		},
		easeInBack: function (x :number, t :number, b :number, c :number, d :number, s :number) {
			if (s == undefined) s = 1.70158;
			return c*(t/=d)*t*((s+1)*t - s) + b;
		},
		easeOutBack: function (x :number, t :number, b :number, c :number, d :number, s :number) {
			if (s == undefined) s = 1.70158;
			return c*((t=t/d-1)*t*((s+1)*t + s) + 1) + b;
		},
		easeInOutBack: function (x :number, t :number, b :number, c :number, d :number, s :number) {
			if (s == undefined) s = 1.70158;
			if ((t/=d/2) < 1) return c/2*(t*t*(((s*=(1.525))+1)*t - s)) + b;
			return c/2*((t-=2)*t*(((s*=(1.525))+1)*t + s) + 2) + b;
		},
	}

	public static getTime(t :Date, options :object) {
		const str = t.toLocaleTimeString('en-gb', options)
		return str;
	}

	public static getDate(t :Date) {

		const str = t.toLocaleDateString('en-gb', { //navigator.language
			month: 'long',
			day: 'numeric'
		})
		return str;
	}

	public static UrlExists(url :string, callback :Function)
	{
		const http = new XMLHttpRequest();
		http.open('HEAD', url);
		http.onreadystatechange = function() {
			if (this.readyState == this.DONE) {
				callback(this.status != 404);
			}
		};
		http.send();
	}

	public static is_cached(src :string) {
		const image = new Image();
		image.src = src;
		return image.complete;
	}

	public static GetColorFromRandomString(str :string) {
		let hash = 0;
		if (str.length === 0) return hash;
		for (let i = 0; i < str.length; i++) {
			hash = str.charCodeAt(i) + ((hash << 5) - hash);
			hash = hash & hash;
		}
		const rgb = [0, 0, 0];
		for (let i = 0; i < 3; i++) {
			const value = (hash >> (i * 8)) & 255;
			rgb[i] = value;
		}
		return rgb;
	}

    constructor() {

    }
}
