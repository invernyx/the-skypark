
import Mapbox from "mapbox-gl"

export default class MapboxExt {

	//https://github.com/mapbox/mapbox-gl-js/issues/2415
	//https://github.com/mapbox/mapbox-gl-js/blob/main/src/ui/camera.js
	public static fitBoundsExt(map: any, bounds :any, options :any, eventData :any) {

		/*
		bounds = Mapbox.LngLatBounds.convert(bounds);
		const p0 = bounds.getNorthWest();
		const p1 = bounds.getSouthEast();

		if(p1.lng - p0.lng > 180) {
			p1.lng = p1.lng - 360;
		}

		const map_padding = map.getPadding();

		const opt = {
			essential: true,
			linear: options.linear !== undefined ? options.linear : null,
			duration: options.duration !== undefined ? options.duration : 1000,
			bearing: options.bearing !== undefined ? options.bearing : 0,
			offset: options.offset !== undefined ? options.offset : [0,0],
			pitch: options.pitch !== undefined ? options.pitch : 0,
			vanish_padding: options.vanish_padding ? {
				left: options.vanish_padding.left !== undefined ? options.vanish_padding.left : map_padding.left,
				right: options.vanish_padding.right !== undefined ? options.vanish_padding.right : map_padding.right,
				top: options.vanish_padding.top !== undefined ? options.vanish_padding.top : map_padding.top,
				bottom: options.vanish_padding.bottom !== undefined ? options.vanish_padding.bottom : map_padding.bottom,
			} : map_padding,
			padding: options.padding ? {
				left: options.padding.left !== undefined ? options.padding.left : 0,
				right: options.padding.right !== undefined ? options.padding.right : 0,
				top: options.padding.top !== undefined ? options.padding.top : 0,
				bottom: options.padding.bottom !== undefined ? options.padding.bottom : 0,
			} : {
				left: 0,
				right: 0,
				top: 0,
				bottom: 0,
			},
			maxZoom: options.maxZoom !== undefined ? options.maxZoom : 1000,
		}

		// Let's find the zoom we'll end up at!
		if(opt.linear == null) {
			const tr = map.transform;
			const edgePadding = opt.vanish_padding;
			const bearing = options.bearing ? options.bearing : 0;

			// We want to calculate the upper right and lower left of the box defined by p0 and p1
			// in a coordinate system rotate to match the destination bearing.
			const p0world = tr.project(Mapbox.LngLat.convert(p0));
			const p1world = tr.project(Mapbox.LngLat.convert(p1));
			const p0rotated = p0world.rotate(-bearing * Math.PI / 180);
			const p1rotated = p1world.rotate(-bearing * Math.PI / 180);

			const upperRight = new Mapbox.Point(Math.max(p0rotated.x, p1rotated.x), Math.max(p0rotated.y, p1rotated.y));
			const lowerLeft = new Mapbox.Point(Math.min(p0rotated.x, p1rotated.x), Math.min(p0rotated.y, p1rotated.y));

			// Calculate zoom: consider the original bbox and padding.
			const size = upperRight.sub(lowerLeft);
			const scaleX = (tr.width - (edgePadding.left + edgePadding.right + opt.padding.left + opt.padding.right)) / size.x;
			const scaleY = (tr.height - (edgePadding.top + edgePadding.bottom + opt.padding.top + opt.padding.bottom)) / size.y;

			if (scaleY < 0 || scaleX < 0) {
				return;
			}

			const zoom = Math.min(tr.scaleZoom(tr.scale * Math.min(scaleX, scaleY)), opt.maxZoom);
			const currentZoom = map.getZoom();

			if(zoom - currentZoom > 5) {
				opt.linear = false;
			} else {
				opt.linear = true;
			}
		}

		map.fitBounds([[p0.lng, p0.lat],[p1.lng, p1.lat]], opt);
		*/



		bounds = Mapbox.LngLatBounds.convert(bounds);

		const p0 = bounds.getNorthWest();
		const p1 = bounds.getSouthEast();

		if(p1.lng - p0.lng > 180) {
			p1.lng = p1.lng - 360;
		}

		if(!options){
			options = {}
		}

		const defaultPadding = {
            top: 0,
            bottom: 0,
            right: 0,
            left: 0
		};

		const bearing = options.bearing ? options.bearing : 0;

		options.maxZoom = options.maxZoom ? options.maxZoom : map.transform.maxZoom;
		options.padding = options.padding ? options.padding : defaultPadding;
		options.offset = options.offset ? options.offset : [0, 0];

        if (typeof options.padding === 'number') {
            const p = options.padding;
            options.padding = {
                top: p,
                bottom: p,
                right: p,
                left: p
            };
        }

        const tr = map.transform;
		const edgePadding = options.vanish ? options.vanish : tr.padding;

        // We want to calculate the upper right and lower left of the box defined by p0 and p1
        // in a coordinate system rotate to match the destination bearing.
        const p0world = tr.project(Mapbox.LngLat.convert(p0));
        const p1world = tr.project(Mapbox.LngLat.convert(p1));
        const p0rotated = p0world.rotate(-bearing * Math.PI / 180);
        const p1rotated = p1world.rotate(-bearing * Math.PI / 180);

        const upperRight = new Mapbox.Point(Math.max(p0rotated.x, p1rotated.x), Math.max(p0rotated.y, p1rotated.y));
		const lowerLeft = new Mapbox.Point(Math.min(p0rotated.x, p1rotated.x), Math.min(p0rotated.y, p1rotated.y));

        // Calculate zoom: consider the original bbox and padding.
        const size = upperRight.sub(lowerLeft);
        const scaleX = (tr.width - (edgePadding.left + edgePadding.right + options.padding.left + options.padding.right)) / size.x;
		const scaleY = (tr.height - (edgePadding.top + edgePadding.bottom + options.padding.top + options.padding.bottom)) / size.y;

        if (scaleY < 0 || scaleX < 0) {
            return;
        }

		const zoom = Math.min(tr.scaleZoom(tr.scale * Math.min(scaleX, scaleY)), options.maxZoom);

        // Calculate center: apply the zoom, the configured offset, as well as offset that exists as a result of padding.
        const offset = (typeof options.offset.x === 'number') ? new Mapbox.Point(options.offset.x, options.offset.y) : Mapbox.Point.convert(options.offset);
        const paddingOffsetX = (options.padding.left - options.padding.right) / 2;
        const paddingOffsetY = (options.padding.top - options.padding.bottom) / 2;
        const paddingOffset = new Mapbox.Point(paddingOffsetX, paddingOffsetY);
        const rotatedPaddingOffset = paddingOffset.rotate(bearing * Math.PI / 180);
        const offsetAtInitialZoom = offset.add(rotatedPaddingOffset);
        const offsetAtFinalZoom = offsetAtInitialZoom.mult(tr.scale / tr.zoomScale(zoom));

        const center = tr.unproject(p0world.add(p1world).div(2).sub(offsetAtFinalZoom));

		options.center = center; //!
		options.zoom = zoom;
		options.padding = options.vanish ? options.vanish : options.padding;

		this.ensurePadding(map, options.padding);

		return options.linear ? map.easeTo(options, eventData) : map.flyTo(options, eventData);

	}

	public static ensurePadding(map :any, new_padding :any) {

		const moveend = () => {
			map.off('moveend', moveend);
			map.setPadding(new_padding);
		}

		map.on('moveend', moveend);
	}

	public static turfCircleFix(geometry :any) {

		let minLon = 0;
		let maxLon = 0;
		let minLat = 0;
		let maxLat = 0;
		geometry.geometry.coordinates.forEach((poly, index) => {

			// Find Min/Max
			poly.forEach(coords => {
				if(minLon > coords[0]) { minLon = coords[0]; }
				if(maxLon < coords[0]) { maxLon = coords[0]; }
				if(minLat > coords[1]) { minLat = coords[1]; }
				if(maxLat < coords[1]) { maxLat = coords[1]; }
				//if(coords[0] > 180) { coords[0] = 180; }
				//if(coords[0] < -180) { coords[0] = -180; }
			});

			// Fix stuff
			let i = 1;
			while(i < poly.length) {
				const coordsPrev = poly[i-1];
				const coords = poly[i];
				if(Math.abs(coordsPrev[0] - coords[0]) > 180) {
					geometry.geometry.type = "MultiLineString";
					geometry.geometry.coordinates[index] = poly.sort((a, b) => a[0] - b[0]);
					poly.splice(poly.length, 0, [poly[0][0] + 360, poly[0][1]]);

					if(coords[1] > 0) {
						geometry.geometry.coordinates[index] = poly.sort((a, b) => b[0] - a[0]);
					}
					break;
				}
				i++;
			}

			//geometry[index] = poly.sort((a, b) => a[0] - b[0]);

		});

		return geometry;
	}

	public static lngFromMercatorX(x: number): number {
		return x * 360 - 180;
	}

	public static latFromMercatorY(y: number): number {
		const y2 = 180 - y * 360;
		return 360 / Math.PI * Math.atan(Math.exp(y2 * Math.PI / 180)) - 90;
	}

	public static globeTileLatLngCorners(id: any): any {
		const tileScale = 1 << id.z;
		const left = id.x / tileScale;
		const right = (id.x + 1) / tileScale;
		const top = id.y / tileScale;
		const bottom = (id.y + 1) / tileScale;

		const latLngTL = [ this.latFromMercatorY(top),  this.lngFromMercatorX(left) ];
		const latLngBR = [ this.latFromMercatorY(bottom), this.lngFromMercatorX(right) ];

		return [latLngTL, latLngBR];
	}

}
