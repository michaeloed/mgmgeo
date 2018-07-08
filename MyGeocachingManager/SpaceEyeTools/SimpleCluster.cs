using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceEyeTools;

namespace SpaceEyeTools
{
    /// <summary>
    /// A simple cluster to hold mutiple waypoints
    /// http://fatdevz.blogspot.fr/2013/06/spatial-data-clustering-with-c.html
    /// </summary>
    public class SimpleCluster
    {
        /// <summary>
        /// Every cluster has to have an ID. This will be used as means of identification of the current cluster.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// list of product names, in order to represent some additional data that you require.
        /// This isn't crucial to the clustering algorithm, but you always need some data to tag along with the object.
        /// </summary>
        public List<string> NAMES { get; set; }

        /// <summary>
        /// This is the latitude and longitude coordinates of representative point of the cluster (in degrees).
        /// </summary>
        public PointDouble LAT_LON_CENTER { get; set; }

        /// <summary>
        /// List of latitude and longitude coordinates participating in the cluster (in degrees)
        /// </summary>
        public List<PointDouble> LAT_LON_LIST { get; set; }

        /// <summary>
        /// Constructor
        /// The first constructor creates a fresh instance of the cluster. 
        /// Something to start out with. 
        /// In the parameters you will see all the data that is needed for the object on first initialization. 
        /// The "awkward" part of this is adding the name and latitude and longitude in the corresponding lists. 
        /// On the first run, your cluster center point is the same as the "only" coordinates in the LAT_LON_LIST list. 
        /// As you remember, this is because at start, each data input entry is treated as a separate cluster. 
        /// This constructor represents that behavior.
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="name">Name</param>
        /// <param name="latitude">Center latitude (in degrees)</param>
        /// <param name="longitude">Center longitude (in degrees)</param>
        public SimpleCluster(int id, string name, double latitude, double longitude)
        {
            ID = id;
            LAT_LON_CENTER = new PointDouble(latitude,longitude);
            NAMES = new List<string>();
            NAMES.Add(name);
            LAT_LON_LIST = new List<PointDouble>();
            LAT_LON_LIST.Add(LAT_LON_CENTER);
        }

        /// <summary>
        /// Constructor
        /// I would like to refer to the second constructor as the "copy" constructor.  
        /// While iterating through the list I came up to a problem of interfering with object data using the new keyword for an copy of the instance and
        /// I needed a quick solution for a deep copy of an object. 
        /// This maybe a bit lazyish to someone, but feel free to implement this any way you want.
        /// </summary>
        /// <param name="old">Cluster to copy</param>
        public SimpleCluster(SimpleCluster old)
        {
            ID = old.ID;
            LAT_LON_CENTER = new PointDouble(old.LAT_LON_CENTER.X,old.LAT_LON_CENTER.Y);
            LAT_LON_LIST = new List<PointDouble>(old.LAT_LON_LIST);
            NAMES = new List<string>(old.NAMES);
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>string value</returns>
        public override string ToString()
        {
            String s = "[" + ID.ToString() + "], count " + LAT_LON_LIST.Count.ToString() + ", centered on " + LAT_LON_CENTER.ToString() + "\r\n";
            foreach (String n in NAMES)
            {
                s += "   " + n + "\r\n";
            }
            return s;
        }

        /// <summary>
        /// Returns area uncluding all clusters elements
        /// </summary>
        /// <returns>4 points: UL, UR, BR, BL (in degrees)</returns>
        public List<PointDouble> GetClusterArea()
        {
            Double LatMin = Double.MaxValue;
            Double LatMax = Double.MinValue;
            Double LonMin = Double.MaxValue;
            Double LonMax = Double.MinValue;
            foreach (PointDouble pt in LAT_LON_LIST)
            {
                if (pt.X < LatMin)
                    LatMin = pt.X;
                if (pt.X > LatMax)
                    LatMax = pt.X;
                if (pt.Y < LonMin)
                    LonMin = pt.Y;
                if (pt.Y > LonMax)
                    LonMax = pt.Y;
            }
            List<PointDouble> area = new List<PointDouble>();
            area.Add(new PointDouble(LatMax, LonMin));
            area.Add(new PointDouble(LatMax, LonMax));
            area.Add(new PointDouble(LatMin, LonMax));
            area.Add(new PointDouble(LatMin, LonMin));
            return area;
        }

        
        /// <summary>
        /// Returns englobing circle area of cluster
        /// </summary>
        /// <param name="center">circle center (in degrees)</param>
        /// <param name="radius">circle radius in kilometers</param>
        public void GetClusterCircle(ref PointDouble center, ref double radius)
        {
            Double x = LAT_LON_LIST[0].X;
            Double y = LAT_LON_LIST[0].Y;

            // Center
            for (int i = 1; i < LAT_LON_LIST.Count; i++)
            {
                x = (x + LAT_LON_LIST[i].X) / 2.0;
                y = (y + LAT_LON_LIST[i].Y) / 2.0;
            }
            center = new PointDouble(x, y);

            // Radius
            radius = 0.0;
            for (int i = 0; i < LAT_LON_LIST.Count; i++)
            {
                Double d = MyTools.DistanceBetweenPoints(x, y, LAT_LON_LIST[i].X, LAT_LON_LIST[i].Y);
                if (d > radius)
                    radius = d;
            }
        }

        /// <summary>
        /// Remove duplicate entries in the cluster
        /// After a call to ClusterTheData, we have duplicate entries in the cluster
        /// </summary>
        private void RemoveDuplicates()
        {
            List<string> newNAMES = new List<string>();
            List<PointDouble> newLAT_LON_LIST = new List<PointDouble>();
            for (int i = 0; i < NAMES.Count; i++)
            {
                String name = NAMES[i];
                if (!newNAMES.Contains(name))
                {
                    newNAMES.Add(name);
                    newLAT_LON_LIST.Add(LAT_LON_LIST[i]);
                }
            }
            NAMES = newNAMES;
            LAT_LON_LIST = newLAT_LON_LIST;
        }

        /// <summary>
        /// This small piece of code represents the generalized algorithm. 
        /// The clusterList is loaded from some source, and we have the settings for the latitude and logitude sensitivity. 
        /// This is the data that is needed to be passed to the ClusterTheData method which will, like the name says, cluster the data. 
        /// As mentioned, you can surround that method with some kind of a loop if you need multiple clusterizations of the data.
        /// The latitude and longitude sensitivity represents the means to calculate the outer bounds of the cluster square area. 
        /// Using the sensitivity variables and the center point of the cluster, it is easy to calculate the outer bounds of the cluster.
        /// Technically we are going through two loops. 
        /// One is going through the list of items that we received as a list of data objects and the second one is looping through the temporary dictionary list of clusters. 
        /// Through each iteration of the first loop we alter the dictionary in some way. 
        /// In its base, if the current item belongs to a cluster, join the data from the currently inspected item to the cluster, 
        /// remove the old reference in the dictionary and add the combined object as the new cluster. 
        /// In case there aren't any cluster that the current item belongs to, create a new item in the dictionary list. 
        /// Every time a new cluster is created or the current one is altered, recalculate the current latitude and longitude center 
        /// point of the cluster and set it as the new position in the coordinate system.
        /// 
        /// I am assuming that you will need new unique IDs when dealing with the resulting list. 
        /// The only important thing here is for the ID to be unique so the clustering would work. 
        /// For the unique ID you could implement a function that gives the combined cluster a new ID based on your business logic. 
        /// Another approach maybe assigning the IDs when looping through the resulting list after the clustering was done. 
        /// In the end, you may have noticed that you can reiterate through the entire algorithm again with the resulting dictionary cluster list. 
        /// Of course, that would mean that you are combining those clusters in a bigger cluster, so in accordance to that,
        /// you would have to change the sensitivity of the clusters, because with the fixed latitude and longitude sensitivities
        /// you would receive a result that is pretty much the same as previous one.
        /// The end result of the algorithm gives you a list of items which you can store and edit for later or immediate use.
        /// 
        /// Complexity
        /// The time complexity shouldn't be too bad for this algorithm. 
        /// You are passing through the entire list once and through each iteration you pass through the list of cluster. 
        /// In worst case scenario it would be something around n*k! where the n is the number of items in the list and k is the number of clusters. 
        /// In worst case scenario it would be n*n which doesn't make much sense because it would mean that data wasn't clustered at all and we don't want that now, do we?.
        /// </summary>
        /// <param name="clusterList">initial source, each element is a cluster</param>
        /// <param name="latitudeSensitivity">latitude outer bounds of the cluster square area (in degrees)</param>
        /// <param name="longitutdeSensitivity">longitude outer bounds of the cluster square area (in degrees)</param>
        /// <returns>final list of clusters</returns>
        static public Dictionary<int, SimpleCluster> ClusterTheData(List<SimpleCluster> clusterList, double latitudeSensitivity, double longitutdeSensitivity)
        {
            //CLUSTER DICTIONARY
            var clusterDictionary = new Dictionary<int, SimpleCluster>();

            //Add the first node to the cluster list
            if (clusterList.Count > 0)
            {
                clusterDictionary.Add(clusterList[0].ID, clusterList[0]);
            }

            //ALGORITHM
            for (int i = 1; i < clusterList.Count; i++)
            {
                SimpleCluster combinedCluster = null;
                SimpleCluster oldCluster = null;
                foreach (var clusterDict in clusterDictionary)
                {
                    //Check if the current item belongs to any of the existing clusters
                    if (CheckIfInCluster(clusterDict.Value, clusterList[i], latitudeSensitivity, longitutdeSensitivity))
                    {
                        //If it belongs to the cluster then combine them and copy the cluster to oldCluster variable;
                        combinedCluster = CombineClusters(clusterDict.Value, clusterList[i]);
                        oldCluster = new SimpleCluster(clusterDict.Value);
                    }
                }

                //This check means that no suitable clusters were found to combine, so the current item in the list becomes a new cluster.
                if (combinedCluster == null)
                {
                    //Adding new cluster to the cluster dictionary 
                    clusterDictionary.Add(clusterList[i].ID, clusterList[i]);
                }
                else
                {
                    //We have created a combined cluster. Now it is time to remove the old cluster from the dictionary and instead of it add a new cluster.
                    clusterDictionary.Remove(oldCluster.ID);
                    clusterDictionary.Add(combinedCluster.ID, combinedCluster);
                }
            }

            // Remove duplicates (filtering)
            foreach (KeyValuePair<int, SimpleCluster> pair in clusterDictionary)
            {
                pair.Value.RemoveDuplicates();
            }
            return clusterDictionary;
        }

        /// <summary>
        /// This small piece of code represents the generalized algorithm. 
        /// The clusterList is loaded from some source, and we have the settings for the latitude and logitude sensitivity. 
        /// This is the data that is needed to be passed to the ClusterTheData method which will, like the name says, cluster the data. 
        /// As mentioned, you can surround that method with some kind of a loop if you need multiple clusterizations of the data.
        /// Technically we are going through two loops. 
        /// One is going through the list of items that we received as a list of data objects and the second one is looping through the temporary dictionary list of clusters. 
        /// Through each iteration of the first loop we alter the dictionary in some way. 
        /// In its base, if the current item belongs to a cluster, join the data from the currently inspected item to the cluster, 
        /// remove the old reference in the dictionary and add the combined object as the new cluster. 
        /// In case there aren't any cluster that the current item belongs to, create a new item in the dictionary list. 
        /// Every time a new cluster is created or the current one is altered, recalculate the current latitude and longitude center 
        /// point of the cluster and set it as the new position in the coordinate system.
        /// 
        /// I am assuming that you will need new unique IDs when dealing with the resulting list. 
        /// The only important thing here is for the ID to be unique so the clustering would work. 
        /// For the unique ID you could implement a function that gives the combined cluster a new ID based on your business logic. 
        /// Another approach maybe assigning the IDs when looping through the resulting list after the clustering was done. 
        /// In the end, you may have noticed that you can reiterate through the entire algorithm again with the resulting dictionary cluster list. 
        /// Of course, that would mean that you are combining those clusters in a bigger cluster, so in accordance to that,
        /// you would have to change the sensitivity of the clusters, because with the fixed latitude and longitude sensitivities
        /// you would receive a result that is pretty much the same as previous one.
        /// The end result of the algorithm gives you a list of items which you can store and edit for later or immediate use.
        /// 
        /// Complexity
        /// The time complexity shouldn't be too bad for this algorithm. 
        /// You are passing through the entire list once and through each iteration you pass through the list of cluster. 
        /// In worst case scenario it would be something around n*k! where the n is the number of items in the list and k is the number of clusters. 
        /// In worst case scenario it would be n*n which doesn't make much sense because it would mean that data wasn't clustered at all and we don't want that now, do we?.
        /// </summary>
        /// <param name="clusterList">initial source, each element is a cluster</param>
        /// <param name="radius">radius for outer bounds of the cluster circle area (in degrees)</param>
        /// <returns>final list of clusters</returns>
        static public Dictionary<int, SimpleCluster> ClusterTheData(List<SimpleCluster> clusterList, double radius)
        {
            //CLUSTER DICTIONARY
            var clusterDictionary = new Dictionary<int, SimpleCluster>();

            //Add the first node to the cluster list
            if (clusterList.Count > 0)
            {
                clusterDictionary.Add(clusterList[0].ID, clusterList[0]);
            }

            //ALGORITHM
            for (int i = 1; i < clusterList.Count; i++)
            {
                SimpleCluster combinedCluster = null;
                SimpleCluster oldCluster = null;
                foreach (var clusterDict in clusterDictionary)
                {
                    //Check if the current item belongs to any of the existing clusters
                    if (CheckIfInCluster(clusterDict.Value, clusterList[i], radius))
                    {
                        //If it belongs to the cluster then combine them and copy the cluster to oldCluster variable;
                        combinedCluster = CombineClusters(clusterDict.Value, clusterList[i]);
                        oldCluster = new SimpleCluster(clusterDict.Value);
                    }
                }

                //This check means that no suitable clusters were found to combine, so the current item in the list becomes a new cluster.
                if (combinedCluster == null)
                {
                    //Adding new cluster to the cluster dictionary 
                    clusterDictionary.Add(clusterList[i].ID, clusterList[i]);
                }
                else
                {
                    //We have created a combined cluster. Now it is time to remove the old cluster from the dictionary and instead of it add a new cluster.
                    clusterDictionary.Remove(oldCluster.ID);
                    clusterDictionary.Add(combinedCluster.ID, combinedCluster);
                }
            }

            // Remove duplicates (filtering)
            foreach (KeyValuePair<int, SimpleCluster> pair in clusterDictionary)
            {
                pair.Value.RemoveDuplicates();
            }
            return clusterDictionary;
        }

        /// <summary>
        /// This is the check I have used for testing if an item belongs to a cluster. 
        /// In its essence, from the center point of the current cluster in the cluster dictionary list, 
        /// create an rectangle area using the latitude and longitude sensitivities. 
        /// The sensitivity is added left and right from the longitude, and up and down to the latitude. 
        /// A fact that is worth mentioning is that this technique is used quite often in 2D games programming.
        /// </summary>
        /// <param name="home">cluster to check if imposter is include in</param>
        /// <param name="imposter">cluster to check if included in home</param>
        /// <param name="latitudeSensitivity">latitude outer bounds of the cluster square area</param>
        /// <param name="longitutdeSensitivity">longitude outer bounds of the cluster square area</param>
        /// <returns>true if item belongs to a cluster</returns>
        static public bool CheckIfInCluster(SimpleCluster home, SimpleCluster imposter, double latitudeSensitivity, double longitutdeSensitivity)
        {
            if ((home.LAT_LON_CENTER.X + latitudeSensitivity) > imposter.LAT_LON_CENTER.X
                   && (home.LAT_LON_CENTER.X - latitudeSensitivity) < imposter.LAT_LON_CENTER.X
                   && (home.LAT_LON_CENTER.Y + longitutdeSensitivity) > imposter.LAT_LON_CENTER.Y
                   && (home.LAT_LON_CENTER.Y - longitutdeSensitivity) < imposter.LAT_LON_CENTER.Y
               )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// This is the check I have used for testing if an item belongs to a cluster. 
        /// In its essence, from the center point of the current cluster in the cluster dictionary list, 
        /// create an circle area using the radius. 
        /// </summary>
        /// <param name="home">cluster to check if imposter is include in</param>
        /// <param name="imposter">cluster to check if included in home</param>
        /// <param name="radius">radius of the cluster circle</param>
        /// <returns>true if item belongs to a cluster</returns>
        static public bool CheckIfInCluster(SimpleCluster home, SimpleCluster imposter, double radius)
        {
            return MyTools.PointInCircle(home.LAT_LON_CENTER.X, home.LAT_LON_CENTER.Y, radius, imposter.LAT_LON_CENTER.X, imposter.LAT_LON_CENTER.Y);
        }

        /// <summary>
        /// As the method name suggests this is the place where combining of the clusters occurs. 
        /// This idea is simple. Assign the home cluster as a combined cluster, add the data from the imposter cluster to the combined cluster. 
        /// Probably some filtering of the data is required, and I am leaving that to the reader to figure that out and design it to his/hers needs. 
        /// The important part is the re-calibration of the center point of the cluster. 
        /// You have added new points to the cluster and because of that you have to recalculate the center point. 
        /// For example, points might be on the one of the edges, and you are just making sure that the cluster is respecting that dynamically, by following the group.
        /// Also notice that the combined cluster receives the home cluster ID. 
        /// This ID (even if not unique compared to the original list) is enough for the algorithm to function, and I didn't have the need of special IDs.
        /// </summary>
        /// <param name="home">cluster that will be combined with imposter</param>
        /// <param name="imposter">cluster that will be combined into home</param>
        /// <returns>compined cluster</returns>
        public static SimpleCluster CombineClusters(SimpleCluster home, SimpleCluster imposter)
        {
            //Deep copy of the home object
            var combinedCluster = new SimpleCluster(home);
            combinedCluster.LAT_LON_LIST.AddRange(imposter.LAT_LON_LIST);
            combinedCluster.NAMES.AddRange(imposter.NAMES);

            //Combine the data of both clusters
            combinedCluster.LAT_LON_LIST.AddRange(imposter.LAT_LON_LIST);
            combinedCluster.NAMES.AddRange(imposter.NAMES);

            //Recalibrate the new center
            combinedCluster.LAT_LON_CENTER = new PointDouble(
                ((home.LAT_LON_CENTER.X + imposter.LAT_LON_CENTER.X) / 2.0),
                ((home.LAT_LON_CENTER.Y + imposter.LAT_LON_CENTER.Y) / 2.0));

            return combinedCluster;
        }
    }

}
