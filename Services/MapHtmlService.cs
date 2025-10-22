using Point_v1.Models;

namespace Point_v1.Services;

public class MapHtmlService
{
    public string GenerateMapHtml(List<MapEvent> events, double centerLat = 55.7558, double centerLon = 37.6173)
    {
        var eventsJson = System.Text.Json.JsonSerializer.Serialize(events);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Карта событий</title>
    <script src='https://api-maps.yandex.ru/2.1/?lang=ru_RU&amp;apikey=3550421c-56bd-406c-bbb1-1eda751ee0f0' type='text/javascript'></script>
    <style>
        body, html, #map {{
            width: 100%; 
            height: 100%; 
            padding: 0; 
            margin: 0;
            font-family: Arial, sans-serif;
        }}
    </style>
</head>
<body>
    <div id='map'></div>
    
    <script type='text/javascript'>
        ymaps.ready(init);
        
        var events = {eventsJson};
        
        function init() {{
            var map = new ymaps.Map('map', {{
                center: [{centerLat}, {centerLon}],
                zoom: 10,
                controls: ['zoomControl', 'fullscreenControl']
            }});
            
            // Добавляем метки событий
            events.forEach(function(event) {{
                if (event.Latitude && event.Longitude) {{
                    var placemark = new ymaps.Placemark([
                        event.Latitude, 
                        event.Longitude
                    ], {{
                        balloonContentHeader: event.Title,
                        balloonContentBody: event.Description + '<br><br>' +
                                          '📅 ' + event.DateDisplay + '<br>' +
                                          '📍 ' + event.Address + '<br>' +
                                          '🎯 ' + event.ParticipantsCount + ' участников',
                        balloonContentFooter: 'Категория: ' + event.CategoryId
                    }}, {{
                        preset: 'islands#violetIcon'
                    }});
                    
                    map.geoObjects.add(placemark);
                }}
            }});
        }}
    </script>
</body>
</html>";
    }
}