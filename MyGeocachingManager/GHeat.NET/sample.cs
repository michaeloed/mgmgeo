
gheat.PointManager pm = new gheat.PointManager();
Graphics g = null;
System.Drawing.Bitmap tempImage = null;
int zoom = 4;
int startX = 2;
int startY = 5;
int maxX = startX + 10;
int maxY = startY + 10;
System.Drawing.Bitmap canvasImage = new  System.Drawing.Bitmap(maxX * 256 - (startX * 256), maxY * 256 - (startY * 256), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
	
String gheatpath = GetResourcesDataPath() + Path.DirectorySeparatorChar + "GH.res" + Path.DirectorySeparatorChar;
gheat.Settings.BaseDirectory = gheatpath;
g = Graphics.FromImage(canvasImage);

//pm.LoadPointsFromFile("points.txt");

List<Geocache> caches = GetDisplayedCaches();
foreach(Geocache geo in caches)
{
	pm.AddPoint(new PointLatLngH(geo._dLatitude, geo._dLongitude));
}

for(int x = startX; x <= maxX; x++)
{
	for(int y = startY; y <= maxY; y++)
	{
		tempImage = gheat.GHeat.GetTile(pm, "classic", zoom, x, y);
		g.DrawImage(tempImage, new System.Drawing.PointF(x * 256 - (startX * 256), y * 256 - (startY * 256)));
	}
}
  
canvasImage.Save("toto.png");