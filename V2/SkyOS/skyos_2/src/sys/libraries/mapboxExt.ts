
import Mapbox from "mapbox-gl"

export default class MapboxExt {

	//https://github.com/mapbox/mapbox-gl-js/issues/2415
	//https://github.com/mapbox/mapbox-gl-js/blob/main/src/ui/camera.js
	public static fitBoundsExt(map: any, bounds :any, options :any, eventData :any) {

		bounds = Mapbox.LngLatBounds.convert(bounds);
		const p0 = bounds.getNorthWest();
		const p1 = bounds.getSouthEast();

		if(p1.lng - p0.lng > 180) {
			p1.lng = p1.lng - 360;
		}

		const opt = {
			essential: true
		}

		if(options) {
			if(options.duration !== undefined) { opt['duration'] = options.duration; }
			if(options.pitch !== undefined) { opt['pitch'] = options.pitch; }
			if(options.bearing !== undefined) { opt['bearing'] = options.bearing; }
			if(options.padding !== undefined) { opt['padding'] = options.padding; }
			if(options.offset !== undefined) { opt['offset'] = options.offset; }
		}

		map.fitBounds([[p0.lng, p0.lat],[p1.lng, p1.lat]], opt);

		/*
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
		const edgePadding = defaultPadding;

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

		return options.linear ? map.easeTo(options, eventData) : map.flyTo(options, eventData);
		*/
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

}
