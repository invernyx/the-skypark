using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace TSP_Transponder
{
    public class Utils
    {
        private static Random rnd = new Random();

        internal static double GetRandom()
        {
            lock (rnd)
            {
                return rnd.NextDouble();
            }
        }

        internal static double GetRandom(double max)
        {
            lock (rnd)
            {
                return rnd.NextDouble() * max;
            }
        }

        internal static int GetRandom(int min)
        {
            lock (rnd)
            {
                return rnd.Next(min);
            }
        }

        internal static int GetRandom(int min, int max)
        {
            lock (rnd)
            {
                return rnd.Next(min, max);
            }
        }

        internal static double GetRandom(double min, double max)
        {
            lock (rnd)
            {
                return ((double)rnd.Next((int)Math.Round(min * 1000000), (int)Math.Round(max * 1000000))) / 1000000;
            }
        }

        internal static void CopyDir(string Origin, string Dest)
        {
            try
            {
                DirectoryInfo OriginDirInfo = new DirectoryInfo(Origin);
                foreach (string OldPath in Directory.GetFiles(Origin, "*.*", SearchOption.AllDirectories))
                {
                    string RelativeNewPath = OldPath.Replace(OriginDirInfo.FullName, "");
                    string AbsoluteNewPath = Path.Combine(Dest, RelativeNewPath);
                    DirectoryInfo Parent = Directory.GetParent(AbsoluteNewPath);
                    if (!Directory.Exists(Parent.FullName))
                    {
                        Directory.CreateDirectory(Parent.FullName);
                    }
                    File.Copy(OldPath, AbsoluteNewPath, true);
                }
            }
            catch
            {
            }
        }

        public static List<double> ParseDMS(string input, int decimals)
        {
            MatchCollection parts = new Regex(@"(\d*\.)?\d+", RegexOptions.None).Matches(input);
            MatchCollection direction = new Regex(@"(N|S|W|E)", RegexOptions.None).Matches(input);
            
            if(direction.Count > 0)
            {
                double lat = ConvertDMSToDD(Convert.ToDouble(parts[0].Value), Convert.ToDouble(parts[1].Value), Convert.ToDouble(parts[2].Value), direction[0].Value);
                double lng = ConvertDMSToDD(Convert.ToDouble(parts[3].Value), Convert.ToDouble(parts[4].Value), Convert.ToDouble(parts[5].Value), direction[1].Value);
                return new List<double>() { Math.Round(lat, decimals), Math.Round(lng, decimals) };
            }
            else
            {
                return new List<double>() { Math.Round(Convert.ToDouble(parts[1].Value), decimals), Math.Round(Convert.ToDouble(parts[0].Value), decimals) };
            }
        }

        public static double ConvertDMSToDD(double degrees, double minutes, double seconds, string direction)
        {
            var dd = degrees + (minutes / 60) + (seconds / (60 * 60));

            if (direction == "S" || direction == "W")
            {
                dd = dd * -1;
            }
            return dd;
        }
        
        public static long TimeStamp(DateTime dt)
        {
            return (long)(dt - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
        }

        public static DateTime TSToDate(long ts)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(ts).ToLocalTime();
            return dtDateTime;
        }
        
        public static double Limiter(double min, double max, double value)
        {
            if (value > max)
            {
                return max;
            }
            else if (value < min)
            {
                return min;
            }
            else
            {
                return value;
            }
        }

        public static double Normalize180(double deg)
        {
            double normalizedDeg = deg % 360;

            if (normalizedDeg <= -180)
                normalizedDeg += 360;
            else if (normalizedDeg > 180)
                normalizedDeg -= 360;

            return normalizedDeg;
        }

        public static double Normalize360(double deg)
        {
            double normalizedDeg = deg;

            if (normalizedDeg < 0)
            {
                normalizedDeg += 360;
            }
            else if (normalizedDeg > 360)
            {
                normalizedDeg -= 360;
            }

            return normalizedDeg;
        }
        
        public static string CleanupPath(string path)
        {
            List<string> pathsplit = path.Split('\\').ToList();
            string endpath = "";
            int i = 0;
            while (i < pathsplit.Count)
            {
                if (pathsplit[i] == string.Empty)
                {
                    i++;
                    continue;
                }
                else
                {
                    string newpart = pathsplit[i].Trim('/').Trim('\\');
                    if (i == 0)
                    {
                        endpath += newpart;
                    }
                    else
                    {
                        endpath += "\\" + newpart;
                    }
                    i++;
                }
            }

            return endpath;
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
        
        public static string GetHash1(string Input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(Input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static List<string> ReadCFG(string CFGPath)
        {
            if (CFGPath != string.Empty)
            {
                List<string> cfg_content = File.ReadAllLines(CFGPath).ToList();

                if (cfg_content != null)
                {
                    return cfg_content;
                }
                else
                {
                    return new List<string>();
                }
            }
            else
            {
                return new List<string>();
            }

        }
        
        public static Dictionary<string, Dictionary<string, List<string>>> DecompileCFG(List<string> CFG)
        {
            Dictionary<string, Dictionary<string, List<string>>> CFGBreakdown = new Dictionary<string, Dictionary<string, List<string>>>();

            string SectionName = "";
            string[] SectionSplit = new string[0];
            Dictionary<string, List<string>> Section = null;
            int LineCount = CFG.Count();
            int LineAt = 0;
            foreach (string Line in CFG)
            {
                if (Line.Trim().StartsWith("["))
                {
                    SectionName = Line.Trim().Replace("[", "").Replace("]", "").Split('/')[0].Trim();
                    SectionSplit = SectionName.Split('.');

                    if (!CFGBreakdown.ContainsKey(SectionSplit[0]))
                    {
                        Section = new Dictionary<string, List<string>>();
                        if (SectionSplit.Count() > 1)
                        {
                            Section.Add(SectionSplit[1], new List<string>());
                        }
                        else
                        {
                            Section.Add("", new List<string>());
                        }
                        CFGBreakdown.Add(SectionSplit[0], Section);
                    }
                    else
                    {
                        if (SectionSplit.Count() > 1)
                        {
                            if (!Section.ContainsKey(SectionSplit[1]))
                            {
                                Section.Add(SectionSplit[1], new List<string>());
                            }
                        }

                        CFGBreakdown[SectionSplit[0]] = Section;
                    }
                }
                else if (Section != null)
                {
                    if (SectionSplit.Count() > 1)
                    {
                        Section[SectionSplit[1]].Add(Line);
                    }
                    else
                    {
                        Section[""].Add(Line);
                    }
                }
                else
                {
                    if (!CFGBreakdown.ContainsKey(string.Empty))
                    {
                        CFGBreakdown.Add("", new Dictionary<string, List<string>>() { { "", new List<string>() } });
                    }

                    CFGBreakdown[""][""].Add(Line);

                }
                LineAt++;
            }

            var lastSection = CFGBreakdown.Values.Last();
            var lastSegment = lastSection.Values.Last();
            if(lastSegment.Count > 0)
            {
                var lastLine = lastSegment.Last();
                if (lastLine != string.Empty)
                {
                    lastSegment.Add("");
                }
            }

            return CFGBreakdown;
        }

        public static KeyValuePair<string, Dictionary<string, List<string>>> GetCFGSection(string key, Dictionary<string, Dictionary<string, List<string>>> AircraftCFG)
        {
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> section in AircraftCFG)
            {
                if (section.Key.ToLower() == key.ToLower())
                {
                    return section;
                }
            }
            return new KeyValuePair<string, Dictionary<string, List<string>>>();
        }
        
        public static bool CompileCFG(string path, Dictionary<string, Dictionary<string, List<string>>> input)
        {
            List<string> output = new List<string>();
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> section in input)
            {
                foreach (KeyValuePair<string, List<string>> segment in section.Value)
                {
                    if(segment.Key != string.Empty)
                    {
                        if(segment.Value.Count > 1)
                        {
                            output.Add("[" + section.Key + "." + segment.Key + "]");
                        }
                        else
                        {
                            output.Add("[" + section.Key + "]");
                        }
                    }

                    output.AddRange(segment.Value);
                }

            }

            if(!File.Exists(path + ".p42.bkp"))
            {
                File.Move(path, path + ".p42.bkp");
            }

            if (File.Exists(path))
            {
                File.WriteAllText(path, string.Join(Environment.NewLine, output), GetEncoding(path));
            }
            else
            {
                File.WriteAllText(path, string.Join(Environment.NewLine, output), Encoding.UTF8);
            }

            return true;
        }

        public static Encoding GetEncoding(string filePath)
        {
            /// <summary>
            /// Determines a text file's encoding by analyzing its byte order mark (BOM).
            /// Defaults to ASCII when detection of the text file's endianness fails.
            /// </summary>
            /// <param name="filename">The text file to analyze.</param>
            /// <returns>The detected encoding.</returns>
            /// 
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.Default;
        }

        public static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        public enum DistanceUnit
        {
            Kilometers,
            Meters,
            Feet,
            NauticalMiles,
            Miles
        }

        public static double MapCalcDist(double lat1, double lon1, double lat2, double lon2, DistanceUnit unit = DistanceUnit.Kilometers, bool UseFloat = false)
        {
            double dist = 0;
            if (UseFloat)
            {
                float rlat1 = 0;
                float rlat2 = 0;
                float theta = 0;
                float rtheta = 0;

                rlat1 = (float)Math.PI * (float)lat1 / 180;
                rlat2 = (float)Math.PI * (float)lat2 / 180;
                theta = (float)lon1 - (float)lon2;
                rtheta = (float)Math.PI * (float)theta / 180;
                dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
                dist = Math.Acos(dist);
                dist = dist * 180 / (float)Math.PI;
                dist = dist * 60 * 1.1515f;

                if (double.IsNaN(dist))
                {
                    return 0;
                }

                switch (unit)
                {
                    case DistanceUnit.Kilometers: //Kilometers -> default
                    return dist * 1.609344f;
                    case DistanceUnit.Meters: //Meters
                    return (dist * 1.609344f) * 1000;
                    case DistanceUnit.Feet: //Feet
                    return (dist * 5280);
                    case DistanceUnit.NauticalMiles: //Nautical Miles 
                    return dist * 0.8684f;
                    case DistanceUnit.Miles: //Miles
                    return dist;
                }
            }
            else
            {
                double rlat1 = 0;
                double rlat2 = 0;
                double theta = 0;
                double rtheta = 0;

                rlat1 = Math.PI * lat1 / 180;
                rlat2 = Math.PI * lat2 / 180;
                theta = lon1 - lon2;
                rtheta = Math.PI * theta / 180;
                dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
                dist = Math.Acos(dist);
                dist = dist * 180 / Math.PI;
                dist = dist * 60 * 1.1515f;

                if (double.IsNaN(dist))
                {
                    return 0;
                }

                switch (unit)
                {
                    case DistanceUnit.Kilometers: //Kilometers -> default
                    return dist * 1.609344f;
                    case DistanceUnit.Meters: //Meters
                    return (dist * 1.609344f) * 1000;
                    case DistanceUnit.Feet: //Feet
                    return (dist * 5280);
                    case DistanceUnit.NauticalMiles: //Nautical Miles 
                    return dist * 0.8684f;
                    case DistanceUnit.Miles: //Miles
                    return dist;
                }
            }

            return dist;
        }

        public static double MapCalcDist(GeoLoc Pos1, GeoLoc Pos2, DistanceUnit unit = DistanceUnit.Kilometers, bool UseFloat = false)
        {
            if(Pos2 != null && Pos1 != null)
            {
                return MapCalcDist(Pos1.Lat, Pos1.Lon, Pos2.Lat, Pos2.Lon, unit, UseFloat);
            }
            else
            {
                return 0;
            }
        }

        public static GeoLoc MapOffsetPosition(double lon, double lat, double distanceMeters, double heading)
        {
            const double R = 6378.14; //Earth Radious in KM
            double distanceKM = Limiter(0, R * 1000 / 2, distanceMeters) / 1000;

            double latitude1 = lat * (Math.PI / 180);
            double longitude1 = lon * (Math.PI / 180);
            double brng = heading * (Math.PI / 180);
            double hdgCos = Math.Cos(Normalize180(heading) * (Math.PI / 180));
            double hdgSin = Math.Round(Math.Sin(Normalize180(heading) * (Math.PI / 180)), 3);
            double distCos = distanceKM * hdgCos;

            double latitude2 = Math.Asin(Math.Sin(latitude1) * Math.Cos(distanceKM / R) + Math.Cos(latitude1) * Math.Sin(distanceKM / R) * Math.Cos(brng));
            double longitude2 = longitude1 + Math.Atan2(Math.Sin(brng) * Math.Sin(distanceKM / R) * Math.Cos(latitude1), Math.Cos(distanceKM / R) - Math.Sin(latitude1) * Math.Sin(latitude2));
            
            latitude2 = latitude2 * (180 / Math.PI);
            longitude2 = longitude2 * (180 / Math.PI);

            if(hdgSin > 0)
            {
                if(longitude2 - lon < 0)
                {
                    longitude2 -= 180;
                }
            }
            else if (hdgSin < 0)
            {
                if (longitude2 - lon > 0)
                {
                    longitude2 += 180;
                }
            }
            
            // Clamp Latitude to 90
            double NorthTrsh = MapCalcDist(lat, lon, 90, lon, DistanceUnit.Kilometers);
            if (NorthTrsh < distCos)
            {
                latitude2 = 90;
            }
            else
            {
                double SouthTrsh = -MapCalcDist(lat, lon, -90, lon, DistanceUnit.Kilometers);
                if (SouthTrsh > distCos)
                {
                    latitude2 = -90;
                }
            }
            
            return new GeoLoc(longitude2, latitude2);
        }

        public static GeoLoc MapOffsetPosition(GeoLoc location, double distanceMeters, double heading)
        {
            return MapOffsetPosition(location.Lon, location.Lat, distanceMeters, heading);
        }
        
        public static double MapCalcBearing(double lat1, double lon1, double lat2, double lon2)
        {
            var dLon = ToRad(lon2 - lon1);
            var dPhi = Math.Log(
                Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI)
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        public static double MapCalcBearing(GeoLoc Pos1, GeoLoc Pos2)
        {
            return MapCalcBearing(Pos1.Lat, Pos1.Lon, Pos2.Lat, Pos2.Lon);
        }

        public static double MapCompareBearings(double initial, double final)
        {
            if (initial > 360 || initial < 0 || final > 360 || final < 0)
            {
                //throw some error
            }

            var diff = final - initial;
            var absDiff = Math.Abs(diff);

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

        public static Vector2 GetClosestPointOnLineSegment(Vector2 A, Vector2 B, Vector2 P)
        {
            Vector2 AP = P - A;       //Vector from A to P   
            Vector2 AB = B - A;       //Vector from A to B  

            float magnitudeAB = AB.LengthSquared();     //Magnitude of AB vector (it's length squared)     
            float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
            float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
            {
                return A;
            }
            else if (distance > 1)
            {
                return B;
            }
            else
            {
                return A + AB * distance;
            }
        }

        public static float DistPointToPoly(List<GeoLoc> vs, GeoLoc point)
        {
            // ray-casting algorithm based on
            // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html

            double x = point.Lon;
            double y = point.Lat;

            bool inside = false;
            float closest = float.MaxValue;
            for (int i = 0, j = vs.Count - 1; i < vs.Count; j = i++)
            {
                double xi = vs[i].Lon;
                double yi = vs[i].Lat;
                double xj = vs[j].Lon;
                double yj = vs[j].Lat;

                if (xi != xj || yi != yj)
                {
                    Vector2 Closest = GetClosestPointOnLineSegment(new Vector2((float)xi, (float)yi), new Vector2((float)xj, (float)yj), new Vector2((float)x, (float)y));
                    float dist = (float)MapCalcDist(Closest.Y, Closest.X, y, x, DistanceUnit.Kilometers, true);
                    if(dist == 0)
                    {

                    }
                    if (dist < closest)
                    {
                        closest = dist;
                    }
                }

                var intersect = ((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                if (intersect) inside = !inside;
            }

            if(inside)
            {
                closest = 0;
            }
            return closest;
        }

        public static bool IsPointInPoly(List<GeoLoc> vs, GeoLoc point)
        {
            // ray-casting algorithm based on
            // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html

            double x = point.Lon;
            double y = point.Lat;

            bool inside = false;
            for (int i = 0, j = vs.Count - 1; i < vs.Count; j = i++)
            {
                double xi = vs[i].Lon;
                double yi = vs[i].Lat;
                double xj = vs[j].Lon;
                double yj = vs[j].Lat;
                
                var intersect = ((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                if (intersect) inside = !inside;
            }
            
            return inside;
        }
        
        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }

        public static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static string FormatNumber(double nm)
        {
            return nm.ToString("n", new CultureInfo("en-US", false)).Replace(".00", "");
        }
        
        public static double CalculateBezierHeightInInterval(double start, double end, double param, double y1, double y2, double y3, double y4)
        {
            return CalculateBezierHeight((param - start) / (end - start), y1, y2, y3, y4);
        }
        
        public static double CalculateBezierHeight(double t, double y1, double y2, double y3, double y4)
        {
            double tPower3 = t * t * t;
            double tPower2 = t * t;
            double oneMinusT = 1 - t;
            double oneMinusTPower3 = oneMinusT * oneMinusT * oneMinusT;
            double oneMinusTPower2 = oneMinusT * oneMinusT;
            double Y = oneMinusTPower3 * y1 + (3 * oneMinusTPower2 * t * y2) + (3 * oneMinusT * tPower2 * y3) + tPower3 * y4;
            return Y;
        }

        public static IEnumerable<string> SplitStr(string str, double chunkSize)
        {
            return Enumerable.Range(0, (int)Math.Ceiling(str.Length / chunkSize))
               .Select(i => new string(str
                   .Skip(i * (int)chunkSize)
                   .Take((int)chunkSize)
                   .ToArray()));
        }

        public static IEnumerable<int> IndexOfAll(string sourceString, string subString)
        {
            return Regex.Matches(sourceString, subString).Cast<Match>().Select(m => m.Index);
        }

        public static GeoLoc GetGeoCurve(double t, GeoLoc p0, GeoLoc p1, GeoLoc p2, GeoLoc p3)
        {
            double cx = 3 * (p1.Lon - p0.Lon);
            double cy = 3 * (p1.Lat - p0.Lat);
            double bx = 3 * (p2.Lon - p1.Lon) - cx;
            double by = 3 * (p2.Lat - p1.Lat) - cy;
            double ax = p3.Lon - p0.Lon - cx - bx;
            double ay = p3.Lat - p0.Lat - cy - by;
            double Cube = t * t * t;
            double Square = t * t;

            double resX = (ax * Cube) + (bx * Square) + (cx * t) + p0.Lon;
            double resY = (ay * Cube) + (by * Square) + (cy * t) + p0.Lat;

            return new GeoLoc(resX, resY);
        }

        public static GeoLoc ClosestPointToSegment(GeoLoc OffsetNode, GeoLoc Node1, GeoLoc Node2)
        {
            //https://stackoverflow.com/questions/3120357/get-closest-point-to-a-line

            GeoLoc a_to_p = new GeoLoc(0,0), a_to_b = new GeoLoc(0,0);
            a_to_p.Lon = OffsetNode.Lon - Node1.Lon;
            a_to_p.Lat = OffsetNode.Lat - Node1.Lat; //     # Storing vector A->P  
            a_to_b.Lon = Node2.Lon - Node1.Lon;
            a_to_b.Lat = Node2.Lat - Node1.Lat; //     # Storing vector A->B

            double atb2 = a_to_b.Lon * a_to_b.Lon + a_to_b.Lat * a_to_b.Lat;
            double atp_dot_atb = a_to_p.Lon * a_to_b.Lon + a_to_p.Lat * a_to_b.Lat; // The dot product of a_to_p and a_to_b
            double t = atp_dot_atb / atb2;  //  # The normalized "distance" from a to the closest point
            if (t >= 1 || t <= 0 || double.IsNaN(t)) return null;
            return new GeoLoc(Node1.Lon + a_to_b.Lon * t, Node1.Lat + a_to_b.Lat * t);
        }

        public static string ConvertESPCountry(string CountryName)
        {
            var Countries = new Dictionary<string, string>() {
                { "afghanistan", "AF" },
                { "albania", "AL" },
                { "united arab emirates", "AE" },
                { "argentina", "AR" },
                { "armenia", "AM" },
                { "australia", "AU" },
                { "austria", "AT" },
                { "azerbaijan", "AZ" },
                { "belgium", "BE" },
                { "bangladesh", "BD" },
                { "bulgaria", "BG" },
                { "bahrain", "BH" },
                { "bosnia and herzegovina", "BA" },
                { "belarus", "BY" },
                { "belize", "BZ" },
                { "bolivia", "BO" },
                { "canada", "CA" },
                { "switzerland", "CH" },
                { "chile", "CL" },
                { "colombia", "CO" },
                { "costa rica", "CR" },
                { "czech republic", "CZ" },
                { "germany", "DE" },
                { "denmark", "DK" },
                { "dominican republic", "DO" },
                { "algeria", "DZ" },
                { "ecuador", "EC" },
                { "egypt", "EG" },
                { "spain", "ES" },
                { "estonia", "EE" },
                { "ethiopia", "ET" },
                { "finland", "FI" },
                { "france", "FR" },
                { "faroe islands", "FO" },
                { "united kingdom", "GB" },
                { "georgia", "GE" },
                { "greece", "GR" },
                { "greenland", "GL" },
                { "guatemala", "GT" },
                { "honduras", "HN" },
                { "croatia", "HR" },
                { "hungary", "HU" },
                { "indonesia", "ID" },
                { "india", "IN" },
                { "ireland", "IE" },
                { "iran", "IR" },
                { "iraq", "IQ" },
                { "iceland", "IS" },
                { "israel", "IL" },
                { "italy", "IT" },
                { "jamaica", "JM" },
                { "jordan", "JO" },
                { "japan", "JP" },
                { "kazakhstan", "KZ" },
                { "kenya", "KE" },
                { "kyrgyzstan", "KG" },
                { "cambodia", "KH" },
                { "korea", "KR" },
                { "kuwait", "KW" },
                { "lao p.d.r.", "LA" },
                { "lebanon", "LB" },
                { "libya", "LY" },
                { "liechtenstein", "LI" },
                { "sri lanka", "LK" },
                { "lithuania", "LT" },
                { "luxembourg", "LU" },
                { "latvia", "LV" },
                { "morocco", "MA" },
                { "principality of monaco", "MC" },
                { "maldives", "MV" },
                { "mexico", "MX" },
                { "macedonia (fyrom)", "MK" },
                { "malta", "MT" },
                { "montenegro", "ME" },
                { "mongolia", "MN" },
                { "malaysia", "MY" },
                { "nigeria", "NG" },
                { "nicaragua", "NI" },
                { "netherlands", "NL" },
                { "norway", "NO" },
                { "nepal", "NP" },
                { "new zealand", "NZ" },
                { "oman", "OM" },
                { "islamic republic of pakistan", "PK" },
                { "panama", "PA" },
                { "peru", "PE" },
                { "poland", "PL" },
                { "puerto rico", "PR" },
                { "portugal", "PT" },
                { "paraguay", "PY" },
                { "qatar", "QA" },
                { "romania", "RO" },
                { "russia", "RU" },
                { "rwanda", "RW" },
                { "saudi arabia", "SA" },
                { "serbia and montenegro (former)", "CS" },
                { "senegal", "SN" },
                { "singapore", "SG" },
                { "el salvador", "SV" },
                { "serbia", "RS" },
                { "slovakia", "SK" },
                { "slovenia", "SI" },
                { "sweden", "SE" },
                { "syria", "SY" },
                { "tajikistan", "TJ" },
                { "thailand", "TH" },
                { "turkmenistan", "TM" },
                { "trinidad and tobago", "TT" },
                { "tunisia", "TN" },
                { "turkey", "TR" },
                { "taiwan", "TW" },
                { "ukraine", "UA" },
                { "uruguay", "UY" },
                { "united states", "US" },
                { "midway islands", "US" },
                { "kiribati", "KI" },
                { "uzbekistan", "UZ" },
                { "venezuela", "VE" },
                { "wallis and futuna", "WF" },
                { "fiji islands", "FJ" },
                { "johnston atoll", "UM" },
                { "vietnam", "VN" },
                { "yemen", "YE" },
                { "tonga", "TO" },
                { "samoa", "WS" },
                { "niue", "NU" },
                { "cook islands", "CK" },
                { "american samoa", "AS" },
                { "french polynesia", "PF" },
                { "south africa", "ZA" },
                { "cuba", "CU" },
                { "bahamas, the", "BS" },
                { "turks and caicos islands", "TC" },
                { "cayman islands", "KY" },
                { "aruba", "AW" },
                { "netherlands antilles", "AN" },
                { "virgin islands, u.s.", "VI" },
                { "virgin islands, british", "VG" },
                { "guadeloupe", "GP" },
                { "antigua and barbuda", "AG" },
                { "dominica", "DM" },
                { "martinique", "MQ" },
                { "saint kitts and nevis", "KN" },
                { "saint lucia", "LC" },
                { "saint vincent and the grenadines", "VC" },
                { "anguilla", "AI" },
                { "grenada", "GD" },
                { "guyana", "GY" },
                { "bermuda", "BM" },
                { "haiti", "HT" },
                { "antarctica", "AQ" },
                { "barbados", "BB" },
                { "french guiana", "GF" },
                { "falkland islands (islas malvinas)", "FK" },
                { "suriname", "SR" },
                { "western sahara (disputed)", "EH" },
                { "saint pierre and miquelon", "PM" },
                { "mauritania", "MR" },
                { "gibraltar", "GI" },
                { "cape verde", "CV" },
                { "guinea-bissau", "GW" },
                { "mali", "ML" },
                { "guinea", "GN" },
                { "sierra leone", "SL" },
                { "gambia, the", "GM" },
                { "liberia", "LR" },
                { "burkina faso", "BF" },
                { "ghana", "GH" },
                { "benin", "BJ" },
                { "moldova", "MD" },
                { "serbia and montenegro", "RS" },
                { "macedonia, former yugoslav republic of", "YU" },
                { "ascension island", "SH" },
                { "niger", "NE" },
                { "togo", "TG" },
                { "sao tome and principe", "ST" },
                { "cameroon", "CM" },
                { "equatorial guinea", "GQ" },
                { "chad", "TD" },
                { "gabon", "GA" },
                { "central african republic", "CF" },
                { "cote d'ivoire", "CI" },
                { "congo", "CG" },
                { "sudan", "SD" },
                { "angola", "AO" },
                { "namibia", "NA" },
                { "zambia", "ZM" },
                { "burundi", "BI" },
                { "congo (drc)", "CD" },
                { "tanzania", "TZ" },
                { "lesotho", "LS" },
                { "uganda", "UG" },
                { "cyprus", "CY" },
                { "somalia", "SO" },
                { "eritrea", "ER" },
                { "djibouti", "DJ" },
                { "malawi", "MW" },
                { "comoros", "KM" },
                { "mozambique", "MZ" },
                { "seychelles", "SC" },
                { "madagascar", "MG" },
                { "mayotte", "FR" },
                { "reunion", "FR" },
                { "swaziland", "SZ" },
                { "mauritius", "MU" },
                { "pakistan", "PK" },
                { "myanmar", "MM" },
                { "botswana", "BW" },
                { "china", "CN" },
                { "laos", "LA" },
                { "bhutan", "BT" },
                { "philippines", "PH" },
                { "macao sar", "MO" },
                { "brunei", "BN" },
                { "hong kong sar", "HK" },
                { "north korea", "KP" },
                { "palau", "PW" },
                { "micronesia", "FM" },
                { "guam", "GU" },
                { "papua new guinea", "PG" },
                { "marshall islands", "MH" },
                { "nauru", "NR" },
                { "tuvalu", "TV" },
                { "wake island", "US" },
                { "usa", "US" },
                { "new caledonia", "NC" },
                { "vanuatu", "VU" },
                { "brasil", "BR" },
                { "brazil", "BR" },
                { "scotland", "GB" },
                { "solomon islands", "SB" },
                { "northern mariana islands", "MP" },
                { "palestinian authority", "PS" },
                { "zimbabwe", "ZW" },
            };

            string LCCountryName = CountryName.ToLower();
            if (Countries.ContainsKey(LCCountryName))
            {
                return Countries[LCCountryName];
            }
            else
            {
                return "XX";
            }
        }

        public static int CalculateLevel(double XP)
        {
            // https://gamedev.stackexchange.com/questions/13638/algorithm-for-dynamically-calculating-a-level-based-on-experience-points
            int Level = (int)Math.Floor((Math.Sqrt(100 * ((2 * XP) + 25)) + 50) / 100);
            return Level;
        }

        public static double CalculateXP(double Level)
        {
            // https://gamedev.stackexchange.com/questions/13638/algorithm-for-dynamically-calculating-a-level-based-on-experience-points
            double XP = (Math.Pow(Level, 2) + Level) / 2 * 100 - (Level * 100);
            return XP;
        }
        
        public static int GetBoundsZoomLevel(GeoLoc ne, GeoLoc sw, int mapWidthPx, int mapHeightPx)
        {
            //https://stackoverflow.com/questions/6048975/google-maps-v3-how-to-calculate-the-zoom-level-for-a-given-bounds

            double latFraction = (LatRad(ne.Lat) - LatRad(sw.Lat)) / Math.PI;

            double lngDiff = ne.Lon - sw.Lon;
            double lngFraction = ((lngDiff < 0) ? (lngDiff + 360) : lngDiff) / 360;

            double latZoom = Zoom(mapHeightPx, latFraction);
            double lngZoom = Zoom(mapWidthPx, lngFraction);

            int result = Math.Min((int)latZoom, (int)lngZoom);
            return Math.Min(result, 21);
        }

        private static double LatRad(double lat)
        {
            double sin = Math.Sin(lat * Math.PI / 180);
            double radX2 = Math.Log((1 + sin) / (1 - sin)) / 2;
            return Math.Max(Math.Min(radX2, Math.PI), -Math.PI) / 2;
        }

        private static double Zoom(int mapPx, double fraction)
        {
            double f = Math.Floor(Math.Log(mapPx / 512 / fraction));
            double f1 = f / Math.Log(2);
            return f1;
        }

        //public static string ConvertCountryCodeToString(string CountryCode)
        //{
        //try
        //{
        //    RegionInfo myRI1 = new RegionInfo(CountryCode);
        //    return myRI1.EnglishName;
        //}
        //catch
        //{
        //    return CountryCode;
        //}

        //CultureInfo Found = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList().Find(x =>
        //{
        //    return x.t.ToUpper() == CountryCode;
        //});
        //if(Found != null)
        //{
        //    return Found.EnglishName;
        //}
        //else
        //{
        //    return "unknown country";
        //}
        //}

        public static int GetNumGUID() {
            string output = "";
            while (output.Length < 8) {
                int r = GetRandom(0, 9999);
                output += Math.Floor((r / 9999f) * 9.9999f).ToString();
            }
            return Convert.ToInt32(output);
        }


        public static Bitmap GetScreenshotFromWindow(Rect rect)
        {
            Bitmap bmp = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen((int)rect.Left, (int)rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            return bmp;
        }

        public static Image ResizeImage(Image imgToResize, int width)
        {
            if (imgToResize.Width > width)
            {
                double Ratio = (double)width / (double)imgToResize.Width;
                int newWidth = width;
                int newHeight = (int)Math.Round(Ratio * imgToResize.Height);

                return new Bitmap(imgToResize, newWidth, newHeight);
            }

            return new Bitmap(imgToResize);
        }

        public static byte[] ImageToByteArray(Image imageIn)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(imageIn, typeof(byte[]));
            return xByte;
        }

        public static Image DownloadImageFromUrl(string imageUrl)
        {
            Image image = null;

            try
            {
                System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(imageUrl);
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;

                System.Net.WebResponse webResponse = webRequest.GetResponse();
                Stream stream = webResponse.GetResponseStream();
                image = Image.FromStream(stream);

                webResponse.Close();
            }
            catch
            {
                return null;
            }

            return image;
        }

        public static string RandomString(int size)
        {
            string characters = "123456789ABCDEFGHIJKLMNPQRSTUVWXYZ";
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                result.Append(characters[GetRandom(characters.Length)]);
            }
            return result.ToString();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        
        private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static string Encrypt(string text, string pwd)
        {
            byte[] originalBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = null;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(pwd);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            // Generating salt bytes
            byte[] threeBytes = new byte[4];
            Array.Copy(Encoding.ASCII.GetBytes("76345883459"), threeBytes, 4);
            byte[] saltBytes = threeBytes;

            // Appending salt bytes to original bytes
            byte[] bytesToBeEncrypted = new byte[saltBytes.Length + originalBytes.Length];
            for (int i = 0; i < saltBytes.Length; i++)
            {
                bytesToBeEncrypted[i] = saltBytes[i];
            }
            for (int i = 0; i < originalBytes.Length; i++)
            {
                bytesToBeEncrypted[i + saltBytes.Length] = originalBytes[i];
            }

            encryptedBytes = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            return Convert.ToBase64String(encryptedBytes);
        }

        private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        public static string Decrypt(string decryptedText, string pwd)
        {
            string ds = decryptedText.TrimEnd("\r\n".ToCharArray());// + "==";
            byte[] bytesToBeDecrypted = Convert.FromBase64String(ds);
            if (bytesToBeDecrypted.Length > 0)
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(pwd);

                // Hash the password with SHA256
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] decryptedBytes = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

                // Getting the size of salt
                int _saltSize = 4;

                // Removing salt bytes, retrieving original bytes
                byte[] originalBytes = new byte[decryptedBytes.Length - _saltSize];
                for (int i = _saltSize; i < decryptedBytes.Length; i++)
                {
                    originalBytes[i - _saltSize] = decryptedBytes[i];
                }

                return Encoding.UTF8.GetString(originalBytes);
            }
            else
            {
                return "";
            }
        }
        /*
        private static float X(float t, float x0, float x1, float x2, float x3)
        {
            return (float)(
                x0 * Math.Pow((1 - t), 3) +
                x1 * 3 * t * Math.Pow((1 - t), 2) +
                x2 * 3 * Math.Pow(t, 2) * (1 - t) +
                x3 * Math.Pow(t, 3)
            );
        }
        private static float Y(float t, float y0, float y1, float y2, float y3)
        {
            return (float)(
                y0 * Math.Pow((1 - t), 3) +
                y1 * 3 * t * Math.Pow((1 - t), 2) +
                y2 * 3 * Math.Pow(t, 2) * (1 - t) +
                y3 * Math.Pow(t, 3)
            );
        }

        public static void DrawBezier(float dt, PointF pt0, PointF pt1, PointF pt2, PointF pt3)
        {
            // Draw the curve.
            List<PointF> points = new List<PointF>();
            for (float t = 0.0f; t < 1.0; t += dt)
            {
                points.Add(new PointF(
                    X(t, pt0.X, pt1.X, pt2.X, pt3.X),
                    Y(t, pt0.Y, pt1.Y, pt2.Y, pt3.Y)));
            }

            // Connect to the final point.
            points.Add(new PointF(
                X(1.0f, pt0.X, pt1.X, pt2.X, pt3.X),
                Y(1.0f, pt0.Y, pt1.Y, pt2.Y, pt3.Y)));
        }
        */
    }
    
    public static class Easings
    {
        /// <summary>
        /// Easing Functions enumeration
        /// </summary>
        public enum EasingFunctions
        {
            None,
            Linear,
            QuadraticEaseIn,
            QuadraticEaseOut,
            QuadraticEaseInOut,
            CubicEaseIn,
            CubicEaseOut,
            CubicEaseInOut,
            QuarticEaseIn,
            QuarticEaseOut,
            QuarticEaseInOut,
            QuinticEaseIn,
            QuinticEaseOut,
            QuinticEaseInOut,
            SineEaseIn,
            SineEaseOut,
            SineEaseInOut,
            CircularEaseIn,
            CircularEaseOut,
            CircularEaseInOut,
            ExponentialEaseIn,
            ExponentialEaseOut,
            ExponentialEaseInOut,
            ElasticEaseIn,
            ElasticEaseOut,
            ElasticEaseInOut,
            BackEaseIn,
            BackEaseOut,
            BackEaseInOut,
            BounceEaseIn,
            BounceEaseOut,
            BounceEaseInOut
        }

        /// <summary>
        /// Constant Pi.
        /// </summary>
        private const double PI = Math.PI;

        /// <summary>
        /// Constant Pi / 2.
        /// </summary>
        private const double HALFPI = Math.PI / 2.0f;


        /// <summary>
        /// Interpolate using the specified function.
        /// </summary>
        static public double Interpolate(double p, EasingFunctions function)
        {
            switch (function)
            {
                default:
                case EasingFunctions.Linear: return Linear(p);
                case EasingFunctions.QuadraticEaseOut: return QuadraticEaseOut(p);
                case EasingFunctions.QuadraticEaseIn: return QuadraticEaseIn(p);
                case EasingFunctions.QuadraticEaseInOut: return QuadraticEaseInOut(p);
                case EasingFunctions.CubicEaseIn: return CubicEaseIn(p);
                case EasingFunctions.CubicEaseOut: return CubicEaseOut(p);
                case EasingFunctions.CubicEaseInOut: return CubicEaseInOut(p);
                case EasingFunctions.QuarticEaseIn: return QuarticEaseIn(p);
                case EasingFunctions.QuarticEaseOut: return QuarticEaseOut(p);
                case EasingFunctions.QuarticEaseInOut: return QuarticEaseInOut(p);
                case EasingFunctions.QuinticEaseIn: return QuinticEaseIn(p);
                case EasingFunctions.QuinticEaseOut: return QuinticEaseOut(p);
                case EasingFunctions.QuinticEaseInOut: return QuinticEaseInOut(p);
                case EasingFunctions.SineEaseIn: return SineEaseIn(p);
                case EasingFunctions.SineEaseOut: return SineEaseOut(p);
                case EasingFunctions.SineEaseInOut: return SineEaseInOut(p);
                case EasingFunctions.CircularEaseIn: return CircularEaseIn(p);
                case EasingFunctions.CircularEaseOut: return CircularEaseOut(p);
                case EasingFunctions.CircularEaseInOut: return CircularEaseInOut(p);
                case EasingFunctions.ExponentialEaseIn: return ExponentialEaseIn(p);
                case EasingFunctions.ExponentialEaseOut: return ExponentialEaseOut(p);
                case EasingFunctions.ExponentialEaseInOut: return ExponentialEaseInOut(p);
                case EasingFunctions.ElasticEaseIn: return ElasticEaseIn(p);
                case EasingFunctions.ElasticEaseOut: return ElasticEaseOut(p);
                case EasingFunctions.ElasticEaseInOut: return ElasticEaseInOut(p);
                case EasingFunctions.BackEaseIn: return BackEaseIn(p);
                case EasingFunctions.BackEaseOut: return BackEaseOut(p);
                case EasingFunctions.BackEaseInOut: return BackEaseInOut(p);
                case EasingFunctions.BounceEaseIn: return BounceEaseIn(p);
                case EasingFunctions.BounceEaseOut: return BounceEaseOut(p);
                case EasingFunctions.BounceEaseInOut: return BounceEaseInOut(p);
            }
        }

        /// <summary>
        /// Modeled after the line y = x
        /// </summary>
        static public double Linear(double p)
        {
            return p;
        }

        /// <summary>
        /// Modeled after the parabola y = x^2
        /// </summary>
        static public double QuadraticEaseIn(double p)
        {
            return p * p;
        }

        /// <summary>
        /// Modeled after the parabola y = -x^2 + 2x
        /// </summary>
        static public double QuadraticEaseOut(double p)
        {
            return -(p * (p - 2));
        }

        /// <summary>
        /// Modeled after the piecewise quadratic
        /// y = (1/2)((2x)^2)             ; [0, 0.5)
        /// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
        /// </summary>
        static public double QuadraticEaseInOut(double p)
        {
            if (p < 0.5f)
            {
                return 2 * p * p;
            }
            else
            {
                return (-2 * p * p) + (4 * p) - 1;
            }
        }

        /// <summary>
        /// Modeled after the cubic y = x^3
        /// </summary>
        static public double CubicEaseIn(double p)
        {
            return p * p * p;
        }

        /// <summary>
        /// Modeled after the cubic y = (x - 1)^3 + 1
        /// </summary>
        static public double CubicEaseOut(double p)
        {
            double f = (p - 1);
            return f * f * f + 1;
        }

        /// <summary>	
        /// Modeled after the piecewise cubic
        /// y = (1/2)((2x)^3)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
        /// </summary>
        static public double CubicEaseInOut(double p)
        {
            if (p < 0.5f)
            {
                return 4 * p * p * p;
            }
            else
            {
                double f = ((2 * p) - 2);
                return 0.5f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after the quartic x^4
        /// </summary>
        static public double QuarticEaseIn(double p)
        {
            return p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quartic y = 1 - (x - 1)^4
        /// </summary>
        static public double QuarticEaseOut(double p)
        {
            double f = (p - 1);
            return f * f * f * (1 - p) + 1;
        }

        /// <summary>
        // Modeled after the piecewise quartic
        // y = (1/2)((2x)^4)        ; [0, 0.5)
        // y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
        /// </summary>
        static public double QuarticEaseInOut(double p)
        {
            if (p < 0.5f)
            {
                return 8 * p * p * p * p;
            }
            else
            {
                double f = (p - 1);
                return -8 * f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after the quintic y = x^5
        /// </summary>
        static public double QuinticEaseIn(double p)
        {
            return p * p * p * p * p;
        }

        /// <summary>
        /// Modeled after the quintic y = (x - 1)^5 + 1
        /// </summary>
        static public double QuinticEaseOut(double p)
        {
            double f = (p - 1);
            return f * f * f * f * f + 1;
        }

        /// <summary>
        /// Modeled after the piecewise quintic
        /// y = (1/2)((2x)^5)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
        /// </summary>
        static public double QuinticEaseInOut(double p)
        {
            if (p < 0.5f)
            {
                return 16 * p * p * p * p * p;
            }
            else
            {
                double f = ((2 * p) - 2);
                return 0.5f * f * f * f * f * f + 1;
            }
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave
        /// </summary>
        static public double SineEaseIn(double p)
        {
            return Math.Sin((p - 1) * HALFPI) + 1;
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave (different phase)
        /// </summary>
        static public double SineEaseOut(double p)
        {
            return Math.Sin(p * HALFPI);
        }

        /// <summary>
        /// Modeled after half sine wave
        /// </summary>
        static public double SineEaseInOut(double p)
        {
            return 0.5f * (1 - Math.Cos(p * PI));
        }

        /// <summary>
        /// Modeled after shifted quadrant IV of unit circle
        /// </summary>
        static public double CircularEaseIn(double p)
        {
            return 1 - Math.Sqrt(1 - (p * p));
        }

        /// <summary>
        /// Modeled after shifted quadrant II of unit circle
        /// </summary>
        static public double CircularEaseOut(double p)
        {
            return Math.Sqrt((2 - p) * p);
        }

        /// <summary>	
        /// Modeled after the piecewise circular function
        /// y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
        /// y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
        /// </summary>
        static public double CircularEaseInOut(double p)
        {
            if (p < 0.5f)
            {
                return 0.5f * (1 - Math.Sqrt(1 - 4 * (p * p)));
            }
            else
            {
                return 0.5f * (Math.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
            }
        }

        /// <summary>
        /// Modeled after the exponential function y = 2^(10(x - 1))
        /// </summary>
        static public double ExponentialEaseIn(double p)
        {
            return (p == 0.0f) ? p : Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the exponential function y = -2^(-10x) + 1
        /// </summary>
        static public double ExponentialEaseOut(double p)
        {
            return (p == 1.0f) ? p : 1 - Math.Pow(2, -10 * p);
        }

        /// <summary>
        /// Modeled after the piecewise exponential
        /// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
        /// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
        /// </summary>
        static public double ExponentialEaseInOut(double p)
        {
            if (p == 0.0 || p == 1.0) return p;

            if (p < 0.5f)
            {
                return 0.5f * Math.Pow(2, (20 * p) - 10);
            }
            else
            {
                return -0.5f * Math.Pow(2, (-20 * p) + 10) + 1;
            }
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
        /// </summary>
        static public double ElasticEaseIn(double p)
        {
            return Math.Sin(13 * HALFPI * p) * Math.Pow(2, 10 * (p - 1));
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
        /// </summary>
        static public double ElasticEaseOut(double p)
        {
            return Math.Sin(-13 * HALFPI * (p + 1)) * Math.Pow(2, -10 * p) + 1;
        }

        /// <summary>
        /// Modeled after the piecewise exponentially-damped sine wave:
        /// y = (1/2)*sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
        /// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
        /// </summary>
        static public double ElasticEaseInOut(double p)
        {
            if (p < 0.5f)
            {
                return 0.5f * Math.Sin(13 * HALFPI * (2 * p)) * Math.Pow(2, 10 * ((2 * p) - 1));
            }
            else
            {
                return 0.5f * (Math.Sin(-13 * HALFPI * ((2 * p - 1) + 1)) * Math.Pow(2, -10 * (2 * p - 1)) + 2);
            }
        }

        /// <summary>
        /// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
        /// </summary>
        static public double BackEaseIn(double p)
        {
            return p * p * p - p * Math.Sin(p * PI);
        }

        /// <summary>
        /// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
        /// </summary>	
        static public double BackEaseOut(double p)
        {
            double f = (1 - p);
            return 1 - (f * f * f - f * Math.Sin(f * PI));
        }

        /// <summary>
        /// Modeled after the piecewise overshooting cubic function:
        /// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
        /// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
        /// </summary>
        static public double BackEaseInOut(double p)
        {
            if (p < 0.5f)
            {
                double f = 2 * p;
                return 0.5f * (f * f * f - f * Math.Sin(f * PI));
            }
            else
            {
                double f = (1 - (2 * p - 1));
                return 0.5f * (1 - (f * f * f - f * Math.Sin(f * PI))) + 0.5f;
            }
        }

        /// <summary>
        /// </summary>
        static public double BounceEaseIn(double p)
        {
            return 1 - BounceEaseOut(1 - p);
        }

        /// <summary>
        /// </summary>
        static public double BounceEaseOut(double p)
        {
            if (p < 4 / 11.0f)
            {
                return (121 * p * p) / 16.0f;
            }
            else if (p < 8 / 11.0f)
            {
                return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
            }
            else if (p < 9 / 10.0f)
            {
                return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
            }
            else
            {
                return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
            }
        }

        /// <summary>
        /// </summary>
        static public double BounceEaseInOut(double p)
        {
            if (p < 0.5f)
            {
                return 0.5f * BounceEaseIn(p * 2);
            }
            else
            {
                return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
            }
        }
    }

    
    public class GeoLoc
    {
        public double Lon = 0;
        public double Lat = 0;

        public GeoLoc(double lon, double lat)
        {
            this.Lon = lon;
            this.Lat = lat;
        }

        public GeoLoc(GeoPosition location)
        {
            Lon = location.Lon;
            Lat = location.Lat;
        }

        public override string ToString()
        {
            return Lon + ", " + Lat;
        }

        public string ToString(int decimals)
        {
            return Math.Round(Lon, decimals) + ", " + Math.Round(Lat, decimals);
        }

        public List<double> ToList(int lecimals = 15)
        {
            return new List<double>() { Math.Round(Lon, lecimals), Math.Round(Lat, lecimals) };
        }

        public GeoLoc Copy()
        {
            GeoLoc copy = (GeoLoc)this.MemberwiseClone();
            return copy;
        }
    }

    public struct IntRect
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }


    public class GeoPosition
    {
        public double Lon = 0;
        public double Lat = 0;
        public double Alt = 0;
        public double Hdg = 0;

        public GeoPosition(double _Lon, double _Lat, double _Alt = 0, double _Hdg = 0)
        {
            Lon = _Lon;
            Lat = _Lat;
            Alt = _Alt;
            Hdg = _Hdg;
        }

        public GeoPosition(GeoLoc _Location, double _Alt = 0, double _Hdg = 0)
        {
            Lon = _Location.Lon;
            Lat = _Location.Lat;
            Alt = _Alt;
            Hdg = _Hdg;
        }

        public GeoPosition(GeoPosition _Location, double _Alt = 0, double _Hdg = 0)
        {
            Lon = _Location.Lon;
            Lat = _Location.Lat;
            Alt = _Alt;
            Hdg = _Hdg;
        }

        public override string ToString()
        {
            return "Lon:" + Lon + " Lat:" + Lat + " Alt:" + Alt + " Hdg:" + Hdg;
        }

        public GeoPosition Copy()
        {
            GeoPosition copy = (GeoPosition)this.MemberwiseClone();
            return copy;
        }
    }
    
    public class SexagesimalAngle
    {
        public bool IsNegative { get; set; }
        public int Degrees { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Milliseconds { get; set; }
        
        public static SexagesimalAngle FromDouble(double angleInDegrees)
        {
            //ensure the value will fall within the primary range [-180.0..+180.0]
            while (angleInDegrees < -180.0)
                angleInDegrees += 360.0;

            while (angleInDegrees > 180.0)
                angleInDegrees -= 360.0;

            var result = new SexagesimalAngle();

            //switch the value to positive
            result.IsNegative = angleInDegrees < 0;
            angleInDegrees = Math.Abs(angleInDegrees);

            //gets the degree
            result.Degrees = (int)Math.Floor(angleInDegrees);
            var delta = angleInDegrees - result.Degrees;

            //gets minutes and seconds
            var seconds = (int)Math.Floor(3600.0 * delta);
            result.Seconds = seconds % 60;
            result.Minutes = (int)Math.Floor(seconds / 60.0);
            delta = delta * 3600.0 - seconds;

            //gets fractions
            result.Milliseconds = (int)(1000.0 * delta);

            return result;
        }
        
        public override string ToString()
        {
            var degrees = this.IsNegative
                ? -this.Degrees
                : this.Degrees;

            return string.Format(
                "{0}° {1:00}' {2:00}\"",
                degrees,
                this.Minutes,
                this.Seconds);
        }
        
        public string ToString(string format)
        {
            switch (format)
            {
                case "NS":
                    return string.Format(
                        "{4}{0}° {1:00}' {2:00}\".{3:000}",
                        this.Degrees,
                        this.Minutes,
                        this.Seconds,
                        this.Milliseconds,
                        this.IsNegative ? 'S' : 'N');

                case "WE":
                    return string.Format(
                        "{4}{0}° {1:00}' {2:00}\".{3:000}",
                        this.Degrees,
                        this.Minutes,
                        this.Seconds,
                        this.Milliseconds,
                        this.IsNegative ? 'W' : 'E');

                default:
                    throw new NotImplementedException();
            }
        }
    }

    class WeightedRandom<T>
    {
        private List<Entry> entries = new List<Entry>();
        private double accumulatedWeight;
        
        public int Count
        {
            get
            {
                return entries.Count();
            }
        }

        public double GetWeight(T item)
        {
            Entry e = entries.Find(x => (dynamic)x.item == item);
            if (e != null)
            {
                return e.weight;
            }
            return 0;
        }

        public void AddEntry(T item, double weight)
        {
            accumulatedWeight += weight;
            entries.Add(new Entry
            {
                item = item,
                weight = weight,
                accumulatedWeight = accumulatedWeight
            });
        }

        public void IncrementEntry(T item, double weightOffset)
        {
            Entry e = entries.Find(x => (dynamic)x.item == item);
            if (e != null)
            {
                e.weight += weightOffset;
                if(e.weight < 1) { e.weight = 1; }
            }
        }
        
        public void EditEntry(T item, double weight)
        {
            Entry e = entries.Find(x => (dynamic)x.item == item);
            if(e != null)
            {
                e.item = item;
                e.weight = weight;
            }
        }

        private void Remove(Entry entry)
        {
            entries.Remove(entry);
            accumulatedWeight = 0;
            foreach (var e in entries)
            {
                accumulatedWeight += e.weight;
                e.accumulatedWeight = accumulatedWeight;
            }
        }

        public void Recalculate()
        {
            accumulatedWeight = 0;
            foreach (var Entry in entries)
            {
                accumulatedWeight += Entry.weight;
                Entry.accumulatedWeight = accumulatedWeight;
            }

            var test = entries.OrderByDescending(x => x.weight);
        }

        public T GetRandom(List<T> from = null)
        {
            if(from != null)
            {
                WeightedRandom<T> inner = new WeightedRandom<T>();
                foreach(T subentry in from)
                {
                    Entry e = entries.Find(x => (dynamic)x.item == subentry);
                    if(e != null)
                    {
                        inner.AddEntry(subentry, e.weight);
                    }
                }
                return inner.GetRandom();
            }
            else
            {
                double r = accumulatedWeight * Utils.GetRandom();
                foreach (Entry entry in entries)
                {
                    if (entry.accumulatedWeight >= r)
                    {
                        return entry.item;
                    }
                }
            }

            return default(T); //should only happen when there are no entries
        }

        public List<T> GetRandoms(int Count)
        {
            List<T> rets = new List<T>();

            while(rets.Count < Count && entries.Count > 0)
            {
                double r = accumulatedWeight * Utils.GetRandom();
                int i = 0;
                while(i < entries.Count)
                {
                    Entry entry = entries[i];
                    if (entry.accumulatedWeight >= r)
                    {
                        rets.Add(entry.item);
                        Remove(entry);
                        i--;
                        break;
                    }
                    i++;
                }
            }

            return rets;
        }


        private class Entry
        {
            public double accumulatedWeight;
            public double weight;
            public T item;

            public override string ToString()
            {
                return weight + " - " + item;
            }
        }
    }
}
