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
    <script src='https://api-maps.yandex.ru/2.1/?lang=ru_RU&amp;apikey=1a0b162d-9aa4-4d51-8441-151469a3c82a' type='text/javascript'></script>
    <style>
        body, html, #map {{
            width: 100%; 
            height: 100%; 
            padding: 0; 
            margin: 0;
            font-family: Arial, sans-serif;
        }}
        .event-info {{
            background: white;
            border-radius: 10px;
            padding: 15px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
            max-width: 300px;
        }}
        .event-title {{
            font-weight: bold;
            font-size: 16px;
            margin-bottom: 8px;
            color: #512BD4;
        }}
        .event-details {{
            font-size: 14px;
            color: #666;
            margin-bottom: 10px;
        }}
        .event-button {{
            background: #512BD4;
            color: white;
            border: none;
            padding: 8px 15px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
            width: 100%;
        }}
        .event-button:hover {{
            background: #3a1d9c;
        }}
    </style>
</head>
<body>
    <div id='map'></div>
    
    <script type='text/javascript'>
        ymaps.ready(init);
        
        var events = {eventsJson};
        var selectedEventId = '';
        
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
                        balloonContentHeader: '<div class=""event-title"">' + event.Title + '</div>',
                        balloonContentBody: 
                            '<div class=""event-details"">' +
                            '📅 ' + event.DateDisplay + '<br>' +
                            '📍 ' + event.Address + '<br>' +
                            '🎯 ' + event.ParticipantsCount + ' участников<br>' +
                            '🏷️ ' + event.CategoryId +
                            '</div>' +
                            '<button class=""event-button"" onclick=""openEventDetails(\'' + event.EventId + '\')"">Перейти к событию</button>',
                        balloonContentFooter: ''
                    }}, {{
                        preset: 'islands#violetIcon',
                        balloonCloseButton: true,
                        hideIconOnBalloonOpen: false
                    }});
                    
                    // Обработчик клика по метке
                    placemark.events.add('click', function (e) {{
                        selectedEventId = event.EventId;
                        console.log('🎯 Клик по событию: ' + event.EventId);
                    }});
                    
                    map.geoObjects.add(placemark);
                }}
            }});
        }}
        
        // Функция для открытия деталей события
        function openEventDetails(eventId) {{
            console.log('🚀 Переход к событию: ' + eventId);
            // Отправляем сообщение в C# код
            if (window.chrome && window.chrome.webview) {{
                window.chrome.webview.postMessage(eventId);
            }} else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webviewHandler) {{
                window.webkit.messageHandlers.webviewHandler.postMessage(eventId);
            }} else {{
                // Fallback для других платформ
                window.location = 'pointapp://event/' + eventId;
            }}
        }}
        
        // Глобальная функция для C# вызовов
        window.getSelectedEventId = function() {{
            return selectedEventId;
        }};
    </script>
</body>
</html>";
    }
}