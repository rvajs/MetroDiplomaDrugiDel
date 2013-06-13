using Bing.Maps;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MetroDiplomaDrugiDel.RouteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Bing.Maps;

namespace MetroDiplomaDrugiDel.ViewModel
{
    public class BingMapsViewModel : ViewModelBase
    {
        public Map MyMap { get; set; }
        public MapLayer NewPolygonLayer { get; set; }

        private MapLayer _routeLayer;
        public MapLayer RouteLayer
        {
            get
            {
                return _routeLayer;
            }
            set
            {
                if (_routeLayer == value)
                    return;
                _routeLayer = value;
                RaisePropertyChanged("RouteLayer");
            }
        }

        private MapLayer _contentPopupLayer;
        public MapLayer ContentPopupLayer
        {
            get
            {
                return _contentPopupLayer;
            }
            set
            {
                if (_contentPopupLayer == value)
                    return;
                _contentPopupLayer = value;
                RaisePropertyChanged("ContentPopupLayer");
            }
        }

        private Canvas _contentPopup;
        public Canvas ContentPopup
        {
            get
            {
                return _contentPopup;
            }
            set
            {
                if (_contentPopup == value)
                    return;
                _contentPopup = value;
                RaisePropertyChanged("ContentPopup");
            }
        }

        const string _cStartNaslovText = "Vnesi začetni naslov oz. lokacijo.";
        const string _cEndNaslovText = "Vnesi končen naslov oz. lokacijo.";

        public BingMapsViewModel()
        {
            StartNaslov = _cStartNaslovText;
            EndNaslov = _cEndNaslovText;

            RisanjePotiVisibility = Visibility.Visible;
            IzbiraTockVisibility = Visibility.Collapsed;
            RisanjePotiIsEnabled = false;
            IzbiraTockIsEnabled = true;
            ((RelayCommand)RisanjePotiCommand).RaiseCanExecuteChanged();
            ((RelayCommand)IzbiraTockCommand).RaiseCanExecuteChanged();

            BtnDodajTockeContent = "Dodaj novi poligon";

        }

        /// <summary>
        /// Nastavi MyMap property
        /// </summary>
        /// <param name="map">Objekt za mapo iz XAML</param>
        /// <param name="mapLayer">Objekt za mapl layer iz XAML</param>
        public void NastaviMapProperty(Map map, MapLayer newPolygonLayer, MapLayer routeLayer, MapLayer contentPopupLayer, Canvas contentPopup)
        {
            MyMap = map;
            NewPolygonLayer = newPolygonLayer;
            RouteLayer = routeLayer;
            ContentPopupLayer = contentPopupLayer;
            ContentPopup = contentPopup;

            // osvežim gumba za preklop med risanjem točk in izrisom poti
            ((RelayCommand)RisanjePotiCommand).RaiseCanExecuteChanged();
            ((RelayCommand)IzbiraTockCommand).RaiseCanExecuteChanged();

            // Doda točko lokacije v polygon za vsak miškin klik na mapi
            MyMap.MouseClick += MyMap_MouseClick;
            MyMap.MouseEnter += MyMap_MouseEnter;
            MyMap.MouseLeave += MyMap_MouseLeave;
        }

        private bool _risanjePotiIsEnabled;
        public bool RisanjePotiIsEnabled
        {
            get
            {
                return _risanjePotiIsEnabled;
            }
            set
            {
                if (_risanjePotiIsEnabled == value)
                    return;
                _risanjePotiIsEnabled = value;
                RaisePropertyChanged("RisanjePotiIsEnabled");
            }
        }

        private bool _izbiraTockIsEnabled;
        public bool IzbiraTockIsEnabled
        {
            get
            {
                return _izbiraTockIsEnabled;
            }
            set
            {
                if (_izbiraTockIsEnabled == value)
                    return;
                _izbiraTockIsEnabled = value;
                RaisePropertyChanged("IzbiraTockIsEnabled");
            }
        }

        private Visibility _risanjePotiVisibility;
        public Visibility RisanjePotiVisibility
        {
            get
            {
                return _risanjePotiVisibility;
            }
            set
            {
                if (_risanjePotiVisibility == value)
                    return;
                _risanjePotiVisibility = value;
                RaisePropertyChanged("RisanjePotiVisibility");
            }
        }

        private Visibility _izbiraTockVisibility;
        public Visibility IzbiraTockVisibility
        {
            get
            {
                return _izbiraTockVisibility;
            }
            set
            {
                if (_izbiraTockVisibility == value)
                    return;
                _izbiraTockVisibility = value;
                RaisePropertyChanged("IzbiraTockVisibility");
            }
        }

        private ICommand _risanjePotiCommand;
        public ICommand RisanjePotiCommand
        {
            get
            {
                if (_risanjePotiCommand == null)
                    _risanjePotiCommand = new RelayCommand(() =>
                    {
                        RisanjePotiVisibility = Visibility.Visible;
                        IzbiraTockVisibility = Visibility.Collapsed;
                        RisanjePotiIsEnabled = false;
                        IzbiraTockIsEnabled = true;
                        ((RelayCommand)RisanjePotiCommand).RaiseCanExecuteChanged();
                        ((RelayCommand)IzbiraTockCommand).RaiseCanExecuteChanged();

                        // reset polygona
                        MyMap.MouseClick -= MyMap_MouseClick;
                        newPolygon = null;
                        polygonPointLayer = null;
                    }, () => RisanjePotiIsEnabled && MyMap != null);
                return _risanjePotiCommand;
            }
        }

        private ICommand _izbiraTockCommand;
        public ICommand IzbiraTockCommand
        {
            get
            {
                if (_izbiraTockCommand == null)
                    _izbiraTockCommand = new RelayCommand(() =>
                    {
                        RisanjePotiVisibility = Visibility.Collapsed;
                        IzbiraTockVisibility = Visibility.Visible;
                        RisanjePotiIsEnabled = true;
                        IzbiraTockIsEnabled = false;
                        ((RelayCommand)RisanjePotiCommand).RaiseCanExecuteChanged();
                        ((RelayCommand)IzbiraTockCommand).RaiseCanExecuteChanged();

                        // Doda točko lokacije v polygon za vsak miškin klik na mapi
                        MyMap.MouseClick += MyMap_MouseClick;

                    }, () => IzbiraTockIsEnabled && MyMap != null);
                return _izbiraTockCommand;
            }
        }




        #region RISANJE POTI


        private ICommand _narisiPotCommand;
        public ICommand NarisiPotCommand
        {
            get
            {
                if (_narisiPotCommand == null)
                    _narisiPotCommand = new RelayCommand(() =>
                    {
                        // Initialize the length of the results array. In this sample we have two waypoints.
                        geocodeResults = new GeocodeService.GeocodeResult[2];

                        // Make the two Geocode requests using the values of the text boxes. Also pass the waypoint indexes 
                        // of these two values within the route.
                        Geocode(StartNaslov, 0);
                        Geocode(EndNaslov, 1);

                    }, () => !string.IsNullOrEmpty(StartNaslov) && !string.IsNullOrEmpty(EndNaslov) && StartNaslov != _cStartNaslovText && EndNaslov != _cEndNaslovText);
                return _narisiPotCommand;
            }
        }

        private string _startNaslov;
        public string StartNaslov
        {
            get
            {
                return _startNaslov;
            }
            set
            {
                if (_startNaslov == value)
                    return;
                _startNaslov = value;
                RaisePropertyChanged("StartNaslov");
                ((RelayCommand)NarisiPotCommand).RaiseCanExecuteChanged();
            }
        }

        private string _endNaslov;
        public string EndNaslov
        {
            get
            {
                return _endNaslov;
            }
            set
            {
                if (_endNaslov == value)
                    return;
                _endNaslov = value;
                RaisePropertyChanged("EndNaslov");
                ((RelayCommand)NarisiPotCommand).RaiseCanExecuteChanged();
            }
        }

        private string _contentPopupText;
        public string ContentPopupText
        {
            get
            {
                return _contentPopupText;
            }
            set
            {
                if (_contentPopupText == value)
                    return;
                _contentPopupText = value;
                RaisePropertyChanged("ContentPopupText");
            }
        }

        // This method accepts a geocode query string as well as a ‘waypoint index’, which will be used to track each asynchronous geocode request.
        private void Geocode(string strAddress, int waypointIndex)
        {
            // Create the service variable and set the callback method using the GeocodeCompleted property.
            GeocodeService.GeocodeServiceClient geocodeService = new GeocodeService.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            geocodeService.GeocodeCompleted += new EventHandler<GeocodeService.GeocodeCompletedEventArgs>(geocodeService_GeocodeCompleted);

            // Set the credentials and the geocode query, which could be an address or location.
            GeocodeService.GeocodeRequest geocodeRequest = new GeocodeService.GeocodeRequest();
            geocodeRequest.Credentials = new GeocodeService.Credentials();
            geocodeRequest.Credentials.ApplicationId = ((ApplicationIdCredentialsProvider)MyMap.CredentialsProvider).ApplicationId;
            geocodeRequest.Query = strAddress;

            // Make the asynchronous Geocode request, using the ‘waypoint index’ as 
            //   the user state to track this request and allow it to be identified when the response is returned.
            geocodeService.GeocodeAsync(geocodeRequest, waypointIndex);
        }

        // This is the global internal variable where results are stored. These are accessed later to calculate the route.
        internal GeocodeService.GeocodeResult[] geocodeResults;

        // This is the Geocode request callback method.
        private void geocodeService_GeocodeCompleted(object sender, GeocodeService.GeocodeCompletedEventArgs e)
        {
            // Retrieve the user state of this response (the ‘waypoint index’) to identify which geocode request 
            //   it corresponds to.
            int waypointIndex = System.Convert.ToInt32(e.UserState);

            // Retrieve the GeocodeResult for this response and store it in the global variable geocodeResults, using
            //   the waypoint index to position it in the array.
            geocodeResults[waypointIndex] = e.Result.Results[0];

            // Look at each element in the global gecodeResults array to figure out if more geocode responses still 
            //   need to be returned.

            bool doneGeocoding = true;

            foreach (GeocodeService.GeocodeResult gr in geocodeResults)
            {
                if (gr == null)
                {
                    doneGeocoding = false;
                }
            }

            // If the geocodeResults array is totally filled, then calculate the route.
            if (doneGeocoding)
            {
                //Clear any existing routes
                ClearRoute();

                //Calculate the route
                CalculateRoute(geocodeResults);
            }

        }

        // This method makes the initial CalculateRoute asynchronous request using the results of the Geocode Service.
        private void CalculateRoute(GeocodeService.GeocodeResult[] locations)
        {
            // Create the service variable and set the callback method using the CalculateRouteCompleted property.
            RouteServiceClient routeService = new RouteServiceClient("BasicHttpBinding_IRouteService");
            routeService.CalculateRouteCompleted += new EventHandler<CalculateRouteCompletedEventArgs>(routeService_CalculateRouteCompleted);

            // Set the credentials.
            RouteService.RouteRequest routeRequest = new RouteService.RouteRequest();
            routeRequest.Culture = MyMap.Culture;
            routeRequest.Credentials = new RouteService.Credentials();
            routeRequest.Credentials.ApplicationId = ((ApplicationIdCredentialsProvider)MyMap.CredentialsProvider).ApplicationId;

            // Return the route points so the route can be drawn.
            routeRequest.Options = new RouteService.RouteOptions();
            routeRequest.Options.RoutePathType = RouteService.RoutePathType.Points;

            // Set the waypoints of the route to be calculated using the Geocode Service results stored in the geocodeResults variable.
            routeRequest.Waypoints = new System.Collections.ObjectModel.ObservableCollection<RouteService.Waypoint>();
            foreach (GeocodeService.GeocodeResult result in locations)
            {
                routeRequest.Waypoints.Add(GeocodeResultToWaypoint(result));
            }

            // Make asynchronous call to fetch the data ... pass state object.
            // Make the CalculateRoute asnychronous request.
            routeService.CalculateRouteAsync(routeRequest);
        }

        private RouteService.Waypoint GeocodeResultToWaypoint(GeocodeService.GeocodeResult result)
        {
            RouteService.Waypoint waypoint = new RouteService.Waypoint();
            waypoint.Description = result.DisplayName;
            waypoint.Location = new RouteService.Location();
            waypoint.Location.Latitude = result.Locations[0].Latitude;
            waypoint.Location.Longitude = result.Locations[0].Longitude;
            return waypoint;
        }

        // This is the callback method for the CalculateRoute request.
        private void routeService_CalculateRouteCompleted(object sender, RouteService.CalculateRouteCompletedEventArgs e)
        {

            RouteResponse routeResponse = e.Result;

            // If the route calculate was a success and contains a route, then draw the route on the map.
            if (routeResponse.ResponseSummary.StatusCode != RouteService.ResponseStatusCode.Success)
            {
                //outString = "error routing ... status <" + e.Result.ResponseSummary.StatusCode.ToString() + ">";
            }
            else if (0 == e.Result.Result.Legs.Count)
            {
                //outString = "Cannot find route";
            }
            else
            {

                Color routeColor = Colors.Blue;
                SolidColorBrush routeBrush = new SolidColorBrush(routeColor);
                //outString = "Found route ... coloring route";
                //ToOutput.Foreground = routeBrush;
                MapPolyline routeLine = new MapPolyline();
                routeLine.Locations = new LocationCollection();
                routeLine.Stroke = routeBrush;
                routeLine.Opacity = 0.65;
                routeLine.StrokeThickness = 5.0;
                foreach (RouteService.Location p in routeResponse.Result.RoutePath.Points)
                {
                    routeLine.Locations.Add(new MapControl.Location(p.Latitude, p.Longitude));
                }
                RouteLayer.Children.Add(routeLine);
                LocationRect rect = new LocationRect(routeLine.Locations[0], routeLine.Locations[routeLine.Locations.Count - 1]);

                foreach (RouteService.ItineraryItem itineraryItem in e.Result.Result.Legs[0].Itinerary)
                {
                    Ellipse point = new Ellipse();
                    point.Width = 10;
                    point.Height = 10;
                    point.Fill = new SolidColorBrush(Colors.Red);
                    point.Opacity = 0.65;
                    Microsoft.Maps.MapControl.Location location = new Microsoft.Maps.MapControl.Location(itineraryItem.Location.Latitude, itineraryItem.Location.Longitude);
                    MapLayer.SetPosition(point, location);
                    MapLayer.SetPositionOrigin(point, PositionOrigin.Center);
                    point.Tag = itineraryItem;
                    point.MouseEnter += MyMap_MouseEnter;
                    point.MouseLeave += MyMap_MouseLeave;

                    RouteLayer.Children.Add(point);
                }

                MyMap.SetView(rect);
            }
        }

        private void ClearRoute()
        {
            //Deregister events for the children layer items that had events
            foreach (UIElement child in RouteLayer.Children)
            {
                Ellipse point = child as Ellipse;
                if (point != null)
                {
                    point.MouseEnter -= MyMap_MouseEnter;
                    point.MouseLeave -= MyMap_MouseLeave;
                }
            }

            //Clear any existing routes
            RouteLayer.Children.Clear();
        }

        void MyMap_MouseEnter(object sender, MouseEventArgs e)
        {
            // Show a popup with data about that point
            Ellipse pin = sender as Ellipse;
            if (pin != null)
            {
                var itineraryItem = pin.Tag as ItineraryItem;
                if (itineraryItem != null)
                {
                    Microsoft.Maps.MapControl.Location location = new MapControl.Location(itineraryItem.Location.Latitude, itineraryItem.Location.Longitude);
                    MapLayer.SetPosition(ContentPopup, location);
                    MapLayer.SetPositionOffset(ContentPopup, new Point(15, -50));

                    string contentString = itineraryItem.Text;
                    //Remove tags from the string
                    Regex regex = new Regex("<(.|\n)*?>");
                    contentString = regex.Replace(contentString, string.Empty);
                    ContentPopupText = contentString;
                    ContentPopup.Visibility = Visibility.Visible;
                }
            }
        }

        void MyMap_MouseLeave(object sender, MouseEventArgs e)
        {
            //Hide the popup
            Ellipse point = sender as Ellipse;
            ContentPopup.Visibility = Visibility.Collapsed;
        }

        private void ContentPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            //Show the popup
            ContentPopup.Visibility = Visibility.Visible;
            Canvas.SetZIndex(ContentPopup, 10);
        }

        private void ContentPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            //Show the popup
            ContentPopup.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region IZBIRA TOČK

        private string _btnDodajTockeContent;
        public string BtnDodajTockeContent
        {
            get
            {
                return _btnDodajTockeContent;
            }
            set
            {
                if (_btnDodajTockeContent == value)
                    return;
                _btnDodajTockeContent = value;
                RaisePropertyChanged("BtnDodajTockeContent");
            }
        }

        private ICommand _dodajPoligonCommand;
        public ICommand DodajPoligonCommand
        {
            get
            {
                if (_dodajPoligonCommand == null)
                    _dodajPoligonCommand = new RelayCommand(() =>
                    {
                        // Toggles ON the CreatePolygonMode flag.
                        if (/*((Button)sender).Tag.ToString() == "false"*/BtnDodajTockeContent == "Dodaj novi poligon")
                        {
                            //((Button)sender).Tag = "true";
                            inCreatePolygonMode = true;

                            BtnDodajTockeContent = "Kreiraj";
                            //txtDescription.Visibility = Visibility.Visible;

                            // Clears any objects added to the polygon layer.
                            if (NewPolygonLayer.Children.Count > 0)
                                NewPolygonLayer.Children.Clear();

                            // Adds the layer that contains the polygon points
                            NewPolygonLayer.Children.Add(polygonPointLayer);

                            newPolygon = new MapPolygon();
                            // Defines the polygon fill details
                            newPolygon.Locations = new LocationCollection();
                            newPolygon.Fill = new SolidColorBrush(Colors.Blue);
                            newPolygon.Stroke = new SolidColorBrush(Colors.Green);
                            newPolygon.StrokeThickness = 3;
                            newPolygon.Opacity = 0.8;
                        }
                        //Toggle OFF the CreatePolygonMode flag.
                        else
                        {
                            //((Button)sender).Tag = "false";
                            inCreatePolygonMode = false;

                            BtnDodajTockeContent = "Dodaj novi poligon";
                            //txtDescription.Visibility = Visibility.Collapsed;

                            try
                            {
                                //If there are two or more points, add the polygon layer to the map
                                if (newPolygon.Locations.Count >= 2)
                                {
                                    // Removes the polygon points layer.
                                    polygonPointLayer.Children.Clear();

                                    // Adds the filled polygon layer to the map.
                                    NewPolygonLayer.Children.Add(newPolygon);
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }, () => true);
                return _dodajPoligonCommand;
            }
        }

        // The user defined polygon to add to the map.
        Bing.Maps.MapPolygon newPolygon = null;
        // The map layer containing the polygon points defined by the user.
        Bing.Maps.MapLayer polygonPointLayer = new Bing.Maps.MapLayer();
        // Determines whether the map is accepting user polygon points
        // through single mouse clicks.
        bool inCreatePolygonMode = false;

        void MyMap_MouseClick(object sender, MapMouseEventArgs e)
        {
            //If the map is accepting polygon points, create a point.
            if (inCreatePolygonMode)
            {
                // Creates a location for a single polygon point and adds it to
                // the polygon's point location list.
                Bing.Maps.Location polygonPointLocation = MyMap.ViewportPointToLocation(
                    e.ViewportPoint);
                newPolygon.Locations.Add(polygonPointLocation);

                // A visual representation of a polygon point.
                Windows.UI.Xaml.Shapes.Rectangle r = new Windows.UI.Xaml.Shapes.Rectangle();
                r.Fill = new SolidColorBrush(Colors.Red);
                r.Stroke = new SolidColorBrush(Colors.Yellow);
                r.StrokeThickness = 1;
                r.Width = 8;
                r.Height = 8;

                // Adds a small square where the user clicked, to mark the polygon point.
                polygonPointLayer.AddChild(r, polygonPointLocation);
            }
        }

        void addNewPolygon()
        {
            Bing.Maps.MapPolygon polygon = new Bing.Maps.MapPolygon();
            polygon.Fill = new SolidColorBrush(Colors.Red);
            polygon.Stroke = new SolidColorBrush(Colors.Yellow);
            polygon.StrokeThickness = 5;
            polygon.Opacity = 0.7;
            polygon.Locations = new LocationCollection() { 
        new Microsoft.Maps.MapControl.Location(20, -20), 
        new Microsoft.Maps.MapControl.Location(20, 20), 
        new Microsoft.Maps.MapControl.Location(-20, 20), 
        new Microsoft.Maps.MapControl.Location(-20, -20) };



            MyMap.Children.Add(polygon);
        }

        void addNewPolyline()
        {
            MapPolyline polyline = new MapPolyline();
            polyline.Stroke = new SolidColorBrush(Colors.White);
            polyline.StrokeThickness = 5;
            polyline.Opacity = 0.7;
            polyline.Locations = new LocationCollection() { 
        new Microsoft.Maps.MapControl.Location(10, -10), 
        new Microsoft.Maps.MapControl.Location(10, 10), 
        new Microsoft.Maps.MapControl.Location(-10, -10), 
        new Microsoft.Maps.MapControl.Location(-10, 10) };

            MyMap.Children.Add(polyline);
        }

        #endregion
    }
}
