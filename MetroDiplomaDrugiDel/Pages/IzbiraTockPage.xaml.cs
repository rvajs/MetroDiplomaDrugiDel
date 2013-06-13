using Bing.Maps;
using BingMapsRESTService.Common.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MetroDiplomaDrugiDel.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class IzbiraTockPage : MetroDiplomaDrugiDel.Common.LayoutAwarePage
    {
        public IzbiraTockPage()
        {
            this.InitializeComponent();
            this.locationCollection = new LocationCollection();

            //MyMap.PointerEntered += MyMap_PointerEntered;
            MyMap.Tapped += MyMap_Tapped;
        }

        void MyMap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pos = e.GetPosition(MyMap);
            Bing.Maps.Location location = new Bing.Maps.Location();
            miniTockeShapeLayer = new MapShapeLayer();
            MyMap.TryPixelToLocation(pos, out location);
            GetAddressByLocation(location);
            locationCollection.Add(location);
        }


        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (locationCollection.Count > 0)
            {
                //ClearMap();

                MapShapeLayer shapeLayer = new MapShapeLayer();

                MapPolygon polygon = new MapPolygon();
                polygon.FillColor = Windows.UI.Colors.Blue;
                polygon.Locations = locationCollection;
                shapeLayer.Shapes.Add(polygon);
                MyMap.ShapeLayers.Add(shapeLayer);
            }
        }

        //private bool inCreatePolygonMode;
        private MapPolygon newPolygon;
        private LocationCollection locationCollection;
        MapShapeLayer miniTockeShapeLayer;

        private void ChooseButton_Click(object sender, RoutedEventArgs e)
        {
            //inCreatePolygonMode = true;

            // Clears any objects added to the polygon layer.
            //if (TockeShapeLayer.Shapes.Count > 0)
            //    TockeShapeLayer.Shapes.Clear();

            MapShapeLayer shapeLayer = new MapShapeLayer();
            

            newPolygon = new MapPolygon();
            // Defines the polygon fill details
            //newPolygon.Locations = new LocationCollection() { new Bing.Maps.Location(44, -107), new Location(44, -110), new Location(46, -110), new Location(46, -107) };
            newPolygon.FillColor = Windows.UI.Colors.Red;
            //newPolygon.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            //newPolygon.StrokeThickness = 3;
            //newPolygon.Opacity = 0.8;
            shapeLayer.Shapes.Add(newPolygon);
            MyMap.ShapeLayers.Add(shapeLayer);
        }

        #region GET LOCATION ASYNC

        async private void GetAddressByLocation(Bing.Maps.Location location)
        {
            var urlPart1 = "http://dev.virtualearth.net/REST/v1/Locations/";
            var stringLocation = string.Format("{0},{1}", location.Latitude, location.Longitude);
            var urlPart2 = "?o=json&key=" + MyMap.Credentials;
            var url = string.Format("{0}{1}{2}", urlPart1, stringLocation, urlPart2);

            Uri geocodeRequest = new Uri(url);
            Response r = await GetResponse(geocodeRequest);

            if (r.ResourceSets != null && r.ResourceSets.Count() > 0)
            {
                if (r.ResourceSets[0].Resources != null && r.ResourceSets[0].Resources.Count() > 0)
                {
                    var address = ((BingMapsRESTService.Common.JSON.Location)(r.ResourceSets[0].Resources[0])).Name;
                    System.Diagnostics.Debug.WriteLine("NAslov: " + address);
                }
            }
        }

        private async Task<BingMapsRESTService.Common.JSON.Response> GetResponse(Uri uri)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(uri);
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                return jsonSerializer.ReadObject(stream) as Response;
            }
        }

        #endregion

        #region CLEAR

        private void ClearMap()
        {
            if (MyMap != null)
            {
                if (MyMap.ShapeLayers != null && MyMap.ShapeLayers.Count > 0)
                    MyMap.ShapeLayers.Clear();

                if (locationCollection.Count > 0)
                    locationCollection.Clear();
            }

            //MyMap.Children.Clear();
            //routeLayer.Shapes.Clear();

            //Clear the geocode results ItemSource
            //GeocodeResults.ItemsSource = null;

            //Clear the route instructions
            //RouteResults.DataContext = null;
        }

        private void ClearMapBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearMap();
        }

        #endregion
    }
}
