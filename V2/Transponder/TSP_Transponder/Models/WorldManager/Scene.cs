using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.WorldManager
{
    class Scene
    {
        public string ID = "";
        public string SocketID = "";

        public bool IsVisible = false;
        internal Dictionary<string, List<Scene_Obj>> Features = new Dictionary<string, List<Scene_Obj>>();

        public Scene(string _SocketID, List<Dictionary<string, dynamic>> Features)
        {
            SocketID = _SocketID;

            foreach (Dictionary<string, dynamic> Feature in Features)
            {
                UpdateFeature(Feature);
            }
        }

        public void Show()
        {
            if (!IsVisible)
            {
                IsVisible = true;
                foreach(List<Scene_Obj> Scenes in Features.Values)
                {
                    foreach (Scene_Obj Scene in Scenes)
                    {
                        Scene.Activate();
                    }
                }
            }
        }

        public void Hide()
        {
            if (IsVisible)
            {
                IsVisible = false;
                foreach (List<Scene_Obj> Objects in Features.Values)
                {
                    foreach (Scene_Obj Obj in Objects)
                    {
                        Obj.Deactivate();
                    }
                }
            }
        }

        internal void Clear()
        {
            Hide();
        }

        public void RemoveFeature(string ID)
        {
            if (Features.ContainsKey(ID))
            {
                lock (Features[ID])
                {
                    foreach (Scene_Obj obj in Features[ID])
                    {
                        obj.Deactivate();
                    }

                    Features[ID].Clear();
                    
                    lock (Features)
                    {
                        Features.Remove(ID);
                    }
                }
            }
        }

        public void UpdateFeature(Dictionary<string, dynamic> Feature)
        {
            List<Scene_Obj> ExistingFeature = new List<Scene_Obj>();
            List<Dictionary<string, dynamic>> IncomingFeature = ((ArrayList)Feature["Objects"]).Cast<Dictionary<string, dynamic>>().ToList();
            List<string> ToUpdate = new List<string>();
            List<Scene_Obj> ToRemove = new List<Scene_Obj>();

            // Find if the Feature already exist in the scene
            if (Features.ContainsKey(Feature["ID"]))
            {
                // Loop through existing features and remove them
                ExistingFeature = (List<Scene_Obj>)Features[Feature["ID"]];

                foreach (Scene_Obj Obj in ExistingFeature)
                {
                    //Parallel.ForEach(ExistingFeature, new ParallelOptions()
                    //{
                    //    MaxDegreeOfParallelism = 5
                    //}, 
                    //(Obj) =>
                    //{
                    // If the model name has change, remove
                    if (Feature["ObjectName"] != Obj.File)
                    {
                        Obj.Deactivate();
                        ToRemove.Add(Obj);
                    }
                    // If the UID of the object exists and we're only relocating, skip, else, remove
                    else
                    {
                        if (IncomingFeature.Find(x => Convert.ToString(x["uid"]) == Obj.Name) == null)
                        {
                            Obj.Deactivate();
                            ToRemove.Add(Obj);
                        }
                    }
                    //});
                }

                // Execute the removal
                foreach(Scene_Obj Obj in ToRemove)
                {
                    ExistingFeature.Remove(Obj);
                }
            }
            else
            {
                // Create the new Feature in the Scene
                Features.Add(Feature["ID"], new List<Scene_Obj>());
            }

            // lock the world
            lock (World.WorldBusy)
            {
                // loop through Each incoming features

                foreach (Dictionary<string, dynamic> newObj in IncomingFeature)
                {
                    //Parallel.ForEach(IncomingFeature,
                    //new ParallelOptions()
                    //{
                    //    MaxDegreeOfParallelism = 1
                    //},
                    //(newObj) =>
                    //{
                    // Convert the UID of the object to a string
                    string uid = Convert.ToString(newObj["uid"]);

                    // Find the existing object if it exists
                    Scene_Obj ExistingObject = ExistingFeature.Find(x => x.Name == uid);
                    if (ExistingObject != null)
                    {
                        // Update the object with the new data
                        ExistingObject.CullingDist = Convert.ToSingle(Feature["Culling"]);
                        ExistingObject.Relocate(new GeoPosition(new GeoLoc(Convert.ToDouble(newObj["location"][0]), Convert.ToDouble(newObj["location"][1])), 0, Convert.ToDouble(newObj["hdg"])));
                    }
                    // Create the new object because it doesn't exist
                    else
                    {
                        // Create it
                        Scene_Obj newSceneObj = new Scene_Obj(SocketID, "_lair", new GeoPosition(new GeoLoc(Convert.ToDouble(newObj["location"][0]), Convert.ToDouble(newObj["location"][1])), 0, Convert.ToDouble(newObj["hdg"])), SceneObjType.Dynamic)
                        {
                            Name = uid,
                            File = Feature["ObjectName"],
                            CullingDist = Convert.ToSingle(Feature["Culling"])
                        };

                        // Add it to the Feature
                        lock (Features[Feature["ID"]])
                        {
                            Features[Feature["ID"]].Add(newSceneObj);
                        }

                        // If it's visible, Activate it
                        if (IsVisible)
                        {
                            newSceneObj.Activate();
                        }
                    }

                    //});
                }
            }

        }
    }
}
