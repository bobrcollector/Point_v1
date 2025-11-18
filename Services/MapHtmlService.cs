using Point_v1.Models;

namespace Point_v1.Services;

public class MapHtmlService
{
    public string GenerateMapHtml(List<MapEvent> events, double centerLat = 55.7558, double centerLon = 37.6173, bool showUserLocation = false)
    {
        var eventsJson = System.Text.Json.JsonSerializer.Serialize(events);
        var userLocationLat = centerLat;
        var userLocationLon = centerLon;

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
        #map {{
            position: relative;
        }}
        .map-button {{
            position: absolute;
            top: 10px;
            right: 10px;
            z-index: 100;
            background: #512BD4;
            color: white;
            border: none;
            padding: 10px 15px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
            transition: background 0.3s;
        }}
        .map-button:hover {{
            background: #3a1d9c;
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
    <div id='map'>
        <button class='map-button' onclick='centerOnUserLocation()'>📍 Мое местоположение</button>
    </div>
    
    <script type='text/javascript'>
        ymaps.ready(init);
        
        var events = {eventsJson};
        var selectedEventId = '';
        var map = null;
        var placemarks = [];
        var userLocationMarker = null;
        var showUserLocation = {(showUserLocation ? "true" : "false")};
        var userLocationLat = {userLocationLat};
        var userLocationLon = {userLocationLon};
        
        function init() {{
            map = new ymaps.Map('map', {{
                center: [{centerLat}, {centerLon}],
                zoom: 10,
                controls: ['zoomControl', 'fullscreenControl']
            }});
            
            // Добавляем маркер текущего местоположения пользователя
            if (showUserLocation) {{
                userLocationMarker = new ymaps.Placemark([{centerLat}, {centerLon}], {{
                    balloonContentHeader: '<div class=""event-title"">📍 Ваше местоположение</div>',
                    balloonContentBody: '<div class=""event-details"">Широта: {centerLat:F4}<br>Долгота: {centerLon:F4}</div>',
                    balloonContentFooter: ''
                }}, {{
                    preset: 'islands#blueIcon',
                    balloonCloseButton: true,
                    hideIconOnBalloonOpen: false
                }});
                map.geoObjects.add(userLocationMarker);
            }}
            
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
                    
                    placemarks.push({{
                        placemark: placemark,
                        eventId: event.EventId,
                        lat: event.Latitude,
                        lon: event.Longitude
                    }});
                    
                    map.geoObjects.add(placemark);
                }}
            }});
        }}
        
        // Функция для центрирования карты на местоположение пользователя
        function centerOnUserLocation() {{
            if (map) {{
                // НОВОЕ: Закрываем все открытые балуны перед изменением центра
                try {{
                    map.balloon.close();
                }} catch (e) {{
                    // Игнорируем ошибку, если балуна нет
                }}
                
                map.setCenter([userLocationLat, userLocationLon], 12, {{
                    checkZoomRange: true
                }});
                console.log('📍 Карта центрирована на местоположение пользователя');
            }}
        }}
        
        // Функция для центрирования карты на событии
        function centerMapOnEvent(eventId) {{
            var found = false;
            placemarks.forEach(function(item) {{
                if (item.eventId === eventId) {{
                    // НОВОЕ: Закрываем все открытые балуны перед открытием нового
                    try {{
                        map.balloon.close();
                    }} catch (e) {{
                        // Игнорируем ошибку
                    }}
                    
                    map.setCenter([item.lat, item.lon], 13, {{
                        checkZoomRange: true
                    }});
                    
                    // Даем время на обновление центра перед открытием балуна
                    setTimeout(function() {{
                        try {{
                            item.placemark.balloon.open();
                        }} catch (e) {{
                            console.log('⚠️ Ошибка при открытии балуна: ' + e);
                        }}
                    }}, 100);
                    
                    selectedEventId = eventId;
                    found = true;
                }}
            }});
            if (!found) {{
                console.log('❌ Событие не найдено: ' + eventId);
            }}
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
        
        // Экспортируем функцию для центрирования
        window.centerMapOnEvent = centerMapOnEvent;
    </script>
</body>
</html>";
    }
    
    public string GenerateMapHtmlWithCenter(List<MapEvent> events, string focusedEventId, double centerLat = 55.7558, double centerLon = 37.6173, bool showUserLocation = false)
    {
        // Если есть фокусируемое событие, используем его координаты как центр
        var focusedEvent = events.FirstOrDefault(e => e.EventId == focusedEventId);
        if (focusedEvent != null)
        {
            centerLat = focusedEvent.Latitude;
            centerLon = focusedEvent.Longitude;
        }

        var eventsJson = System.Text.Json.JsonSerializer.Serialize(events);
        // Сохраняем координаты пользователя для кнопки "Мое местоположение"
        var userLocationLat = showUserLocation ? centerLat : 55.7558;
        var userLocationLon = showUserLocation ? centerLon : 37.6173;

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
        #map {{
            position: relative;
        }}
        .map-button {{
            position: absolute;
            top: 10px;
            right: 10px;
            z-index: 100;
            background: #512BD4;
            color: white;
            border: none;
            padding: 10px 15px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
            transition: background 0.3s;
        }}
        .map-button:hover {{
            background: #3a1d9c;
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
    <div id='map'>
        <button class='map-button' onclick='centerOnUserLocation()'>📍 Мое местоположение</button>
    </div>
    
    <script type='text/javascript'>
        ymaps.ready(init);
        
        var events = {eventsJson};
        var selectedEventId = '';
        var map = null;
        var placemarks = [];
        var userLocationMarker = null;
        var focusedEventId = '{focusedEventId}';
        var showUserLocation = {(showUserLocation ? "true" : "false")};
        var userLocationLat = {userLocationLat};
        var userLocationLon = {userLocationLon};
        
        function init() {{
            map = new ymaps.Map('map', {{
                center: [{centerLat}, {centerLon}],
                zoom: focusedEventId ? 13 : 10,
                controls: ['zoomControl', 'fullscreenControl']
            }});
            
            // Добавляем маркер текущего местоположения пользователя
            if (showUserLocation && !focusedEventId) {{
                userLocationMarker = new ymaps.Placemark([{centerLat}, {centerLon}], {{
                    balloonContentHeader: '<div class=""event-title"">📍 Ваше местоположение</div>',
                    balloonContentBody: '<div class=""event-details"">Широта: {centerLat:F4}<br>Долгота: {centerLon:F4}</div>',
                    balloonContentFooter: ''
                }}, {{
                    preset: 'islands#blueIcon',
                    balloonCloseButton: true,
                    hideIconOnBalloonOpen: false
                }});
                map.geoObjects.add(userLocationMarker);
            }}
            
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
                    
                    placemarks.push({{
                        placemark: placemark,
                        eventId: event.EventId,
                        lat: event.Latitude,
                        lon: event.Longitude
                    }});
                    
                    map.geoObjects.add(placemark);
                }}
            }});
            
            // Если есть фокусируемое событие, открываем его балун
            if (focusedEventId) {{
                setTimeout(function() {{
                    centerMapOnEvent(focusedEventId);
                }}, 500);
            }}
        }}
        
        // Функция для центрирования карты на местоположение пользователя
        function centerOnUserLocation() {{
            if (map) {{
                // НОВОЕ: Закрываем все открытые балуны перед изменением центра
                try {{
                    map.balloon.close();
                }} catch (e) {{
                    // Игнорируем ошибку, если балуна нет
                }}
                
                map.setCenter([userLocationLat, userLocationLon], 12, {{
                    checkZoomRange: true
                }});
                console.log('📍 Карта центрирована на местоположение пользователя');
            }}
        }}
        
        // Функция для центрирования карты на событии
        function centerMapOnEvent(eventId) {{
            var found = false;
            placemarks.forEach(function(item) {{
                if (item.eventId === eventId) {{
                    // НОВОЕ: Закрываем все открытые балуны перед открытием нового
                    try {{
                        map.balloon.close();
                    }} catch (e) {{
                        // Игнорируем ошибку
                    }}
                    
                    map.setCenter([item.lat, item.lon], 13, {{
                        checkZoomRange: true
                    }});
                    
                    // Даем время на обновление центра перед открытием балуна
                    setTimeout(function() {{
                        try {{
                            item.placemark.balloon.open();
                        }} catch (e) {{
                            console.log('⚠️ Ошибка при открытии балуна: ' + e);
                        }}
                    }}, 100);
                    
                    selectedEventId = eventId;
                    found = true;
                }}
            }});
            if (!found) {{
                console.log('❌ Событие не найдено: ' + eventId);
            }}
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
        
        // Экспортируем функцию для центрирования
        window.centerMapOnEvent = centerMapOnEvent;
    </script>
</body>
</html>";
    }

    public string GenerateLocationPickerMapHtml(double centerLat, double centerLon, double? selectedLat = null, double? selectedLon = null)
    {
        var selectedLatValue = selectedLat ?? centerLat;
        var selectedLonValue = selectedLon ?? centerLon;

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Выбор местоположения</title>
    <script src='https://api-maps.yandex.ru/2.1/?lang=ru_RU&amp;apikey=1a0b162d-9aa4-4d51-8441-151469a3c82a' type='text/javascript'></script>
    <style>
        body, html, #map {{ 
            width: 100%; 
            height: 100%; 
            padding: 0; 
            margin: 0;
            font-family: Arial, sans-serif;
        }}
        #map {{
            position: relative;
        }}
        .map-button {{
            position: absolute;
            top: 10px;
            right: 10px;
            z-index: 100;
            background: #512BD4;
            color: white;
            border: none;
            padding: 10px 15px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
        }}
        .info-panel {{
            position: absolute;
            bottom: 20px;
            left: 50%;
            transform: translateX(-50%);
            background: white;
            padding: 15px 20px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.3);
            z-index: 100;
            text-align: center;
            min-width: 250px;
        }}
        .info-text {{
            font-size: 14px;
            color: #333;
            margin-bottom: 5px;
        }}
        .coordinates {{
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div id='map'>
        <button class='map-button' onclick='centerOnUserLocation()'>📍 Мое местоположение</button>
        <div class='info-panel' id='infoPanel'>
            <div class='info-text'>Кликните на карте, чтобы выбрать местоположение</div>
            <div class='coordinates' id='coordinates'></div>
        </div>
    </div>
    
    <script type='text/javascript'>
        ymaps.ready(init);
        
        var map = null;
        var selectedPlacemark = null;
        var centerLat = {centerLat};
        var centerLon = {centerLon};
        var selectedLat = {selectedLatValue};
        var selectedLon = {selectedLonValue};
        
        function init() {{
            map = new ymaps.Map('map', {{
                center: [centerLat, centerLon],
                zoom: 13
            }});
            
            // Добавляем метку выбранного местоположения, если она есть
            if (selectedLat && selectedLon) {{
                addPlacemark(selectedLat, selectedLon);
            }}
            
            // Обработчик клика на карте
            map.events.add('click', function (e) {{
                var coords = e.get('coords');
                addPlacemark(coords[0], coords[1]);
                
                // Отправляем координаты в приложение через кастомный протокол
                var lat = coords[0].toFixed(6);
                var lon = coords[1].toFixed(6);
                window.location.href = 'app://location?lat=' + lat + '&lon=' + lon;
            }});
        }}
        
        function addPlacemark(lat, lon) {{
            // Удаляем предыдущую метку
            if (selectedPlacemark) {{
                map.geoObjects.remove(selectedPlacemark);
            }}
            
            // Создаем новую метку
            selectedPlacemark = new ymaps.Placemark([lat, lon], {{
                balloonContent: 'Выбранное местоположение'
            }}, {{
                preset: 'islands#redDotIcon',
                draggable: true
            }});
            
            map.geoObjects.add(selectedPlacemark);
            
            // Обновляем информацию
            updateInfo(lat, lon);
            
            // Обработчик перетаскивания метки
            selectedPlacemark.events.add('dragend', function () {{
                var coords = selectedPlacemark.geometry.getCoordinates();
                updateInfo(coords[0], coords[1]);
                
                // Отправляем координаты в приложение через кастомный протокол
                var lat = coords[0].toFixed(6);
                var lon = coords[1].toFixed(6);
                window.location.href = 'app://location?lat=' + lat + '&lon=' + lon;
            }});
            
            // Центрируем карту на метке
            map.setCenter([lat, lon], map.getZoom());
        }}
        
        function updateInfo(lat, lon) {{
            var coordsElement = document.getElementById('coordinates');
            if (coordsElement) {{
                coordsElement.textContent = lat.toFixed(6) + ', ' + lon.toFixed(6);
            }}
        }}
        
        function centerOnUserLocation() {{
            if (navigator.geolocation) {{
                navigator.geolocation.getCurrentPosition(function(position) {{
                    var lat = position.coords.latitude;
                    var lon = position.coords.longitude;
                    map.setCenter([lat, lon], 15);
                    addPlacemark(lat, lon);
                    
                    // Отправляем координаты в приложение через кастомный протокол
                    var latStr = lat.toFixed(6);
                    var lonStr = lon.toFixed(6);
                    window.location.href = 'app://location?lat=' + latStr + '&lon=' + lonStr;
                }}, function(error) {{
                    alert('Не удалось определить местоположение');
                }});
            }} else {{
                alert('Геолокация не поддерживается');
            }}
        }}
    </script>
</body>
</html>";
    }
}