export default class plan_loader {

    public static LoadPlan(callback :Function) {
		const dlAnchorElem = document.createElement('input');
		dlAnchorElem.style.display = 'none';
		dlAnchorElem.setAttribute('type', 'file');
		dlAnchorElem.setAttribute('accept', '.pln');
		dlAnchorElem.onchange = (evt: Event) => {
			try {
				const files = (evt.target as HTMLInputElement).files;
				if (!files.length) {
					callback(null, null);
					return;
				}
				const file = files[0];
				const reader = new FileReader();
				reader.onload = (event: any) => {
					callback(event.target.result as string, file.name);
				};
				reader.readAsText(file);
			} catch (err) {
				console.error(err);
				callback(null, null);
			}
			document.body.removeChild(dlAnchorElem);
		};
		document.body.appendChild(dlAnchorElem);
		dlAnchorElem.click();
	}

	/*
	private static ReadPln(content :string) :any {
		try {
			const xmlDoc = new DOMParser().parseFromString(content, "text/xml");
			console.log(xmlDoc);

			const SimBaseEL = xmlDoc.getElementsByTagName("SimBase.Document")[0];
			const FlightPlanEL = SimBaseEL.getElementsByTagName("FlightPlan.FlightPlan")[0];
			const ATCWaypointsELs = FlightPlanEL.getElementsByTagName("ATCWaypoint");

			for (const Waypoint of ATCWaypointsELs) {

				const ATCWaypointTypeEL = Waypoint.getElementsByTagName("ATCWaypointType")[0];
				console.log(ATCWaypointTypeEL);
			}
			//document.getElementById("demo").innerHTML =
			//xmlDoc.getElementsByTagName("title")[0].childNodes[0].nodeValue;
			return null;
		} catch {
			console.log("Error reading file");
			return null;
		}
	}
	*/

}