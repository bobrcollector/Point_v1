# Полная документация Point v1

## Содержание

1. [Архитектура приложения](#архитектура-приложения)
2. [Схема базы данных](#схема-базы-данных)
3. [Примеры API](#примеры-api)
4. [Структура проекта](#структура-проекта)
5. [Используемые технологии](#используемые-технологии)
6. [Сервисы и их назначение](#сервисы-и-их-назначение)
7. [ViewModels и их функции](#viewmodels-и-их-функции)
8. [Модели данных](#модели-данных)

---

## Архитектура приложения

### Общая архитектура

Приложение построено на основе **MVVM (Model-View-ViewModel)** паттерна с использованием **Dependency Injection**.

```
┌─────────────────────────────────────────────────────────────┐
│                         Views (UI)                           │
│  (XAML страницы: HomePage, EventDetailsPage, ProfilePage...) │
└───────────────────────┬─────────────────────────────────────┘
                        │ Data Binding
┌───────────────────────▼─────────────────────────────────────┐
│                    ViewModels                                │
│  (HomeViewModel, EventDetailsViewModel, ProfileViewModel...) │
└───────────────────────┬─────────────────────────────────────┘
                        │ Dependency Injection
┌───────────────────────▼─────────────────────────────────────┐
│                      Services                                │
│  (IDataService, IAuthService, IMapService, ISearchService)  │
└───────────────────────┬─────────────────────────────────────┘
                        │ HTTP/REST API
┌───────────────────────▼─────────────────────────────────────┐
│              Firebase Realtime Database                      │
│              Firebase Authentication                         │
│              Firebase Storage                                │
│              Yandex Maps API                                 │
└─────────────────────────────────────────────────────────────┘
```

### Компоненты архитектуры

#### 1. **Views (Представление)**
- XAML файлы для определения UI
- Code-behind файлы для обработки событий
- Используют Data Binding для связи с ViewModels

#### 2. **ViewModels (Модель представления)**
- Наследуются от `BaseViewModel`
- Реализуют `INotifyPropertyChanged` для уведомления об изменениях
- Содержат бизнес-логику и команды
- Не зависят от UI

#### 3. **Services (Сервисы)**
- Абстракции через интерфейсы (IAuthService, IDataService, etc.)
- Реализации для работы с внешними API
- Singleton или Transient в зависимости от назначения

#### 4. **Models (Модели)**
- Классы данных (Event, User, Interest, Report)
- Содержат свойства и вычисляемые поля

### Dependency Injection

Регистрация сервисов происходит в `MauiProgram.cs`:

```csharp
// Сервисы регистрируются как Singleton
builder.Services.AddSingleton<IAuthService, FirebaseAuthService>();
builder.Services.AddSingleton<IDataService, FirestoreDataService>();

// ViewModels регистрируются как Transient
builder.Services.AddTransient<HomeViewModel>();
builder.Services.AddTransient<EventDetailsViewModel>();
```

---

## Схема базы данных

### Firebase Realtime Database структура

```
point-v1-default-rtdb
│
├── events/
│   ├── {eventId}/
│   │   ├── Id: string
│   │   ├── Title: string
│   │   ├── Description: string
│   │   ├── CategoryId: string
│   │   ├── CategoryIds: string[]
│   │   ├── Address: string
│   │   ├── Latitude: double?
│   │   ├── Longitude: double?
│   │   ├── EventDate: DateTime (ISO string)
│   │   ├── CreatedAt: DateTime (ISO string)
│   │   ├── CreatorId: string
│   │   ├── CreatorName: string
│   │   ├── MaxParticipants: int
│   │   ├── ParticipantIds: string[]
│   │   ├── IsActive: bool
│   │   ├── IsBlocked: bool
│   │   ├── BlockedBy: string?
│   │   ├── BlockedAt: DateTime? (ISO string)
│   │   ├── BlockReason: string?
│   │   └── ModerationNotes: string?
│   └── ...
│
├── users/
│   ├── {userId}/
│   │   ├── Id: string
│   │   ├── DisplayName: string
│   │   ├── Email: string
│   │   ├── Role: int (enum: User=0, Organizer=1, Moderator=2, Admin=3)
│   │   ├── IsActive: bool
│   │   ├── CreatedAt: DateTime (ISO string)
│   │   ├── BlockedUntil: DateTime? (ISO string)
│   │   ├── InterestIds: string[]
│   │   ├── City: string?
│   │   ├── About: string?
│   │   ├── AvatarUrl: string?
│   │   ├── UpdatedAt: DateTime (ISO string)
│   │   ├── CreatedEventsCount: int
│   │   └── ParticipatedEventsCount: int
│   └── ...
│
├── interests/
│   ├── {interestId}/
│   │   ├── Id: string
│   │   └── Name: string
│   └── ...
│
├── reports/
│   ├── {reportId}/
│   │   ├── Id: string
│   │   ├── TargetEventId: string
│   │   ├── ReporterUserId: string
│   │   ├── Type: int (enum: Spam=0, Inappropriate=1, Scam=2, Illegal=3, Other=4)
│   │   ├── Reason: string
│   │   ├── Status: int (enum: Pending=0, Approved=1, Rejected=2)
│   │   ├── CreatedAt: DateTime (ISO string)
│   │   ├── ResolvedBy: string?
│   │   ├── ResolvedAt: DateTime? (ISO string)
│   │   └── ModeratorNotes: string?
│   └── ...
│
└── audit_logs/
    ├── {logId}/
    │   ├── Id: string
    │   ├── Action: string
    │   ├── UserId: string
    │   ├── TargetId: string?
    │   ├── Timestamp: DateTime (ISO string)
    │   └── Details: object
    └── ...
```

### Связи между сущностями

- **User ↔ Event**: Один ко многим (один пользователь может создать много событий)
- **User ↔ Event**: Многие ко многим (пользователи могут участвовать в событиях через `ParticipantIds`)
- **User ↔ Interest**: Многие ко многим (пользователи могут иметь несколько интересов)
- **Event ↔ Report**: Один ко многим (одно событие может иметь несколько жалоб)
- **User ↔ Report**: Один ко многим (один пользователь может создать несколько жалоб)

### Индексы и правила безопасности

**Рекомендуемые правила Firebase Realtime Database:**

```json
{
  "rules": {
    "events": {
      ".read": "auth != null",
      ".write": "auth != null",
      "$eventId": {
        ".validate": "newData.hasChildren(['Title', 'CreatorId', 'EventDate'])"
      }
    },
    "users": {
      "$userId": {
        ".read": "$userId === auth.uid || root.child('users').child(auth.uid).child('Role').val() >= 2",
        ".write": "$userId === auth.uid || root.child('users').child(auth.uid).child('Role').val() >= 2"
      }
    },
    "reports": {
      ".read": "root.child('users').child(auth.uid).child('Role').val() >= 2",
      ".write": "auth != null"
    }
  }
}
```

---

## Примеры API

### Firebase Realtime Database API

#### 1. Получение всех событий

**Endpoint:** `GET https://point-v1-default-rtdb.europe-west1.firebasedatabase.app/events.json`

**Пример запроса:**
```http
GET /events.json HTTP/1.1
Host: point-v1-default-rtdb.europe-west1.firebasedatabase.app
```

**Пример ответа:**
```json
{
  "-N1234567890": {
    "Id": "-N1234567890",
    "Title": "Концерт в парке",
    "Description": "Открытый концерт под открытым небом",
    "CategoryId": "Музыка",
    "CategoryIds": ["Музыка", "Развлечения"],
    "Address": "Центральный парк, Москва",
    "Latitude": 55.7558,
    "Longitude": 37.6173,
    "EventDate": "2024-12-25T18:00:00Z",
    "CreatedAt": "2024-12-01T10:00:00Z",
    "CreatorId": "user123",
    "CreatorName": "Иван Иванов",
    "MaxParticipants": 50,
    "ParticipantIds": ["user123", "user456"],
    "IsActive": true,
    "IsBlocked": false
  }
}
```

#### 2. Создание события

**Endpoint:** `POST https://point-v1-default-rtdb.europe-west1.firebasedatabase.app/events.json`

**Пример запроса:**
```http
POST /events.json HTTP/1.1
Host: point-v1-default-rtdb.europe-west1.firebasedatabase.app
Content-Type: application/json

{
  "Title": "Новое событие",
  "Description": "Описание события",
  "CategoryId": "Спорт",
  "CategoryIds": ["Спорт"],
  "Address": "Стадион, Москва",
  "Latitude": 55.7558,
  "Longitude": 37.6173,
  "EventDate": "2024-12-30T15:00:00Z",
  "CreatedAt": "2024-12-01T12:00:00Z",
  "CreatorId": "user123",
  "CreatorName": "Иван Иванов",
  "MaxParticipants": 20,
  "ParticipantIds": [],
  "IsActive": true,
  "IsBlocked": false
}
```

**Пример ответа:**
```json
{
  "name": "-N1234567891"
}
```

#### 3. Обновление события

**Endpoint:** `PUT https://point-v1-default-rtdb.europe-west1.firebasedatabase.app/events/{eventId}.json`

**Пример запроса:**
```http
PUT /events/-N1234567890.json HTTP/1.1
Host: point-v1-default-rtdb.europe-west1.firebasedatabase.app
Content-Type: application/json

{
  "Id": "-N1234567890",
  "Title": "Обновленное событие",
  "MaxParticipants": 30,
  ...
}
```

#### 4. Удаление события

**Endpoint:** `DELETE https://point-v1-default-rtdb.europe-west1.firebasedatabase.app/events/{eventId}.json`

**Пример запроса:**
```http
DELETE /events/-N1234567890.json HTTP/1.1
Host: point-v1-default-rtdb.europe-west1.firebasedatabase.app
```

#### 5. Получение пользователя

**Endpoint:** `GET https://point-v1-default-rtdb.europe-west1.firebasedatabase.app/users/{userId}.json`

**Пример ответа:**
```json
{
  "Id": "user123",
  "DisplayName": "Иван Иванов",
  "Email": "ivan@example.com",
  "Role": 0,
  "IsActive": true,
  "CreatedAt": "2024-01-01T00:00:00Z",
  "InterestIds": ["interest1", "interest2"],
  "City": "Москва",
  "About": "Люблю спорт и музыку",
  "AvatarUrl": "https://firebasestorage.googleapis.com/...",
  "CreatedEventsCount": 5,
  "ParticipatedEventsCount": 12
}
```

### Firebase Authentication API

#### 1. Регистрация пользователя

**Endpoint:** `POST https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={API_KEY}`

**Пример запроса:**
```http
POST /v1/accounts:signUp?key=AIzaSyAEzmKGE5xr4u2ggze_eTuYyKfVr823vJs HTTP/1.1
Host: identitytoolkit.googleapis.com
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "securePassword123",
  "displayName": "Иван Иванов",
  "returnSecureToken": true
}
```

**Пример ответа:**
```json
{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6Ij...",
  "email": "user@example.com",
  "refreshToken": "AEu4IL0...",
  "expiresIn": "3600",
  "localId": "user123",
  "displayName": "Иван Иванов"
}
```

#### 2. Вход пользователя

**Endpoint:** `POST https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={API_KEY}`

**Пример запроса:**
```http
POST /v1/accounts:signInWithPassword?key=AIzaSyAEzmKGE5xr4u2ggze_eTuYyKfVr823vJs HTTP/1.1
Host: identitytoolkit.googleapis.com
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "securePassword123",
  "returnSecureToken": true
}
```

#### 3. Удаление аккаунта

**Endpoint:** `POST https://identitytoolkit.googleapis.com/v1/accounts:delete?key={API_KEY}`

**Пример запроса:**
```http
POST /v1/accounts:delete?key=AIzaSyAEzmKGE5xr4u2ggze_eTuYyKfVr823vJs HTTP/1.1
Host: identitytoolkit.googleapis.com
Content-Type: application/json

{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6Ij..."
}
```

### Yandex Geocoding API

#### 1. Прямое геокодирование (адрес → координаты)

**Endpoint:** `GET https://geocode-maps.yandex.ru/1.x/?format=json&apikey={API_KEY}&geocode={address}`

**Пример запроса:**
```http
GET /1.x/?format=json&apikey=YOUR_API_KEY&geocode=Москва,%20Красная%20площадь HTTP/1.1
Host: geocode-maps.yandex.ru
```

**Пример ответа:**
```json
{
  "response": {
    "GeoObjectCollection": {
      "featureMember": [
        {
          "GeoObject": {
            "Point": {
              "pos": "37.6173 55.7558"
            }
          }
        }
      ]
    }
  }
}
```

#### 2. Обратное геокодирование (координаты → адрес)

**Endpoint:** `GET https://geocode-maps.yandex.ru/1.x/?format=json&apikey={API_KEY}&geocode={longitude},{latitude}`

**Пример запроса:**
```http
GET /1.x/?format=json&apikey=YOUR_API_KEY&geocode=37.6173,55.7558 HTTP/1.1
Host: geocode-maps.yandex.ru
```

### Использование API в коде

#### Пример работы с событиями

```csharp
// Получение всех событий
var dataService = serviceProvider.GetService<IDataService>();
var events = await dataService.GetEventsAsync();

// Создание события
var newEvent = new Event
{
    Title = "Новое событие",
    Description = "Описание",
    EventDate = DateTime.Now.AddDays(7),
    CreatorId = currentUserId,
    MaxParticipants = 20
};
await dataService.AddEventAsync(newEvent);

// Присоединение к событию
await dataService.JoinEventAsync(eventId, userId);
```

#### Пример аутентификации

```csharp
// Регистрация
var authService = serviceProvider.GetService<IAuthService>();
var success = await authService.CreateUser("user@example.com", "password", "Имя");

// Вход
var signedIn = await authService.SignIn("user@example.com", "password");

// Проверка статуса
if (authService.IsAuthenticated)
{
    var userId = authService.CurrentUserId;
}
```

---

## Структура проекта

```
Point_v1/
│
├── Models/                          # Модели данных
│   ├── Event.cs                     # Модель события
│   ├── User.cs                      # Модель пользователя
│   ├── Interest.cs                  # Модель интереса
│   ├── Report.cs                    # Модель жалобы
│   ├── AuditLog.cs                  # Модель лога аудита
│   ├── MapEvent.cs                  # Модель события для карты
│   └── EventFilters.cs              # Фильтры событий
│
├── ViewModels/                      # ViewModels (MVVM)
│   ├── BaseViewModel.cs             # Базовый ViewModel
│   ├── HomeViewModel.cs             # Главная страница
│   ├── EventDetailsViewModel.cs     # Детали события
│   ├── CreateEventViewModel.cs      # Создание события
│   ├── ProfileViewModel.cs          # Профиль пользователя
│   ├── SettingsViewModel.cs         # Настройки
│   ├── MyEventsViewModel.cs         # Мои события
│   ├── FilterViewModel.cs           # Фильтры
│   ├── SearchViewModel.cs           # Поиск
│   ├── ModeratorDashboardViewModel.cs # Панель модератора
│   ├── ReportsManagementViewModel.cs  # Управление жалобами
│   ├── SelectInterestsViewModel.cs   # Выбор интересов
│   ├── MapLocationPickerViewModel.cs # Выбор места на карте
│   └── AuthViewModel.cs             # Аутентификация
│
├── Views/                           # Представления (XAML)
│   ├── HomePage.xaml                # Главная страница
│   ├── EventDetailsPage.xaml        # Детали события
│   ├── CreateEventPage.xaml         # Создание события
│   ├── ProfilePage.xaml             # Профиль
│   ├── SettingsPage.xaml            # Настройки
│   ├── MyEventsPage.xaml            # Мои события
│   ├── FilterPage.xaml              # Фильтры
│   ├── SearchPage.xaml              # Поиск
│   ├── LoginPage.xaml               # Вход
│   ├── RegisterPage.xaml            # Регистрация
│   ├── EditProfilePage.xaml         # Редактирование профиля
│   ├── SelectInterestsPage.xaml     # Выбор интересов
│   ├── MapLocationPickerPage.xaml   # Выбор места
│   ├── ModeratorDashboardPage.xaml  # Панель модератора
│   └── ReportsManagementPage.xaml    # Управление жалобами
│
├── Services/                        # Сервисы
│   ├── IDataService.cs              # Интерфейс работы с данными
│   ├── FirestoreDataService.cs      # Реализация работы с данными
│   ├── FirebaseRestService.cs       # REST клиент для Firebase
│   ├── IAuthService.cs              # Интерфейс аутентификации
│   ├── FirebaseAuthService.cs       # Реализация аутентификации
│   ├── IMapService.cs               # Интерфейс работы с картами
│   ├── MapService.cs                # Реализация работы с картами
│   ├── MapHtmlService.cs            # Генерация HTML для карт
│   ├── YandexGeocodingService.cs    # Геокодирование Yandex
│   ├── ISearchService.cs            # Интерфейс поиска
│   ├── SearchService.cs              # Реализация поиска
│   ├── INavigationService.cs        # Интерфейс навигации
│   ├── NavigationService.cs         # Реализация навигации
│   ├── IReportService.cs            # Интерфейс работы с жалобами
│   ├── ReportService.cs             # Реализация работы с жалобами
│   ├── FilterStateService.cs        # Состояние фильтров
│   ├── MapViewStateService.cs       # Состояние карты
│   ├── AuthStateService.cs          # Состояние аутентификации
│   ├── NavigationStateService.cs    # Состояние навигации
│   ├── CreateEventStateService.cs   # Состояние создания события
│   ├── UserProfileService.cs        # Сервис профиля пользователя
│   ├── LocationSelectionService.cs  # Выбор местоположения
│   ├── AuthorizationService.cs      # Авторизация
│   └── DataService.cs                # Дополнительный сервис данных
│
├── Converters/                      # Конвертеры для Data Binding
│   ├── InterestSelectionConverters.cs
│   ├── IsRelevantToColorConverter.cs
│   ├── PendingReportsColorConverter.cs
│   └── ... (множество других конвертеров)
│
├── Controls/                        # Пользовательские контролы
│   └── AutoCompleteEntry.xaml       # Поле ввода с автодополнением
│
├── Platforms/                       # Платформо-специфичный код
│   ├── Android/
│   │   ├── MainActivity.cs
│   │   ├── MainApplication.cs
│   │   └── AndroidManifest.xml
│   ├── iOS/
│   │   ├── Program.cs
│   │   ├── AppDelegate.cs
│   │   └── Info.plist
│   ├── Windows/
│   │   ├── App.xaml.cs
│   │   └── Package.appxmanifest
│   └── MacCatalyst/
│       ├── Program.cs
│       └── AppDelegate.cs
│
├── Resources/                       # Ресурсы
│   ├── Images/                      # Изображения
│   ├── Fonts/                       # Шрифты
│   └── Styles/                      # Стили
│
├── App.xaml                         # Главный файл приложения
├── App.xaml.cs                      # Code-behind для App
├── AppShell.xaml                    # Shell навигация
├── AppShell.xaml.cs                 # Code-behind для Shell
├── MauiProgram.cs                   # Регистрация сервисов и DI
├── Point_v1.csproj                  # Файл проекта
└── google-services.json              # Конфигурация Firebase
```

---

## Используемые технологии

### Основные технологии

1. **.NET MAUI (Multi-platform App UI)**
   - Версия: .NET 10.0
   - Кроссплатформенный фреймворк для разработки мобильных и десктопных приложений
   - Поддержка Android, iOS, Windows, macOS

2. **Firebase**
   - **Firebase Realtime Database**: NoSQL база данных в реальном времени
   - **Firebase Authentication**: Аутентификация пользователей
   - **Firebase Storage**: Хранение файлов (аватары)

3. **Yandex Maps API**
   - Геокодирование адресов
   - Обратное геокодирование координат
   - Отображение карт

4. **CommunityToolkit.Maui**
   - Версия: 12.2.0
   - Дополнительные контролы и утилиты для MAUI

5. **Newtonsoft.Json**
   - Версия: 13.0.3
   - Сериализация и десериализация JSON

### Архитектурные паттерны

1. **MVVM (Model-View-ViewModel)**
   - Разделение логики и представления
   - Data Binding для связи View и ViewModel

2. **Dependency Injection**
   - Использование встроенного DI контейнера .NET
   - Регистрация в `MauiProgram.cs`

3. **Repository Pattern**
   - Абстракция доступа к данным через интерфейсы
   - `IDataService`, `IAuthService`, `IMapService`

4. **Service Layer**
   - Бизнес-логика вынесена в сервисы
   - Состояние приложения управляется через State Services

### Платформо-специфичные технологии

- **Android**: Android SDK, Material Design
- **iOS**: Xcode, UIKit/SwiftUI bridge
- **Windows**: WinUI 3, UWP
- **macOS**: Mac Catalyst, AppKit

---

## Сервисы и их назначение

### Сервисы данных

#### `IDataService` / `FirestoreDataService`
**Назначение:** Работа с данными событий, пользователей и интересов

**Основные методы:**
- `GetEventsAsync()` - получение всех событий
- `GetEventAsync(string eventId)` - получение события по ID
- `AddEventAsync(Event eventItem)` - создание события
- `UpdateEventAsync(Event eventItem)` - обновление события
- `DeleteEventAsync(string eventId)` - удаление события
- `JoinEventAsync(string eventId, string userId)` - присоединение к событию
- `LeaveEventAsync(string eventId, string userId)` - выход из события
- `GetUserAsync(string userId)` - получение пользователя
- `UpdateUserAsync(User user)` - обновление пользователя

#### `FirebaseRestService`
**Назначение:** Низкоуровневая работа с Firebase REST API

**Основные методы:**
- `SignInWithEmailAndPassword()` - вход
- `CreateUserWithEmailAndPassword()` - регистрация
- `DeleteAccountAsync()` - удаление аккаунта
- `GetEventsAsync()` - получение событий
- `AddEventAsync()` - создание события
- `UpdateEventAsync()` - обновление события
- `DeleteEventAsync()` - удаление события
- `GetUsersAsync()` - получение пользователей
- `AddOrUpdateUserAsync()` - сохранение пользователя
- `UploadAvatarAsync()` - загрузка аватара

### Сервисы аутентификации

#### `IAuthService` / `FirebaseAuthService`
**Назначение:** Управление аутентификацией пользователей

**Основные методы:**
- `CreateUser()` - регистрация
- `SignIn()` - вход
- `SignOut()` - выход
- `DeleteAccountAsync()` - удаление аккаунта

**Свойства:**
- `IsAuthenticated` - статус аутентификации
- `CurrentUserId` - ID текущего пользователя

**События:**
- `AuthStateChanged` - изменение статуса аутентификации

#### `IAuthStateService` / `AuthStateService`
**Назначение:** Управление состоянием аутентификации

### Сервисы карт и геолокации

#### `IMapService` / `MapService`
**Назначение:** Работа с картами и геолокацией

**Основные методы:**
- `GetCurrentLocationAsync()` - получение текущей локации
- Работа с картами событий

#### `MapHtmlService`
**Назначение:** Генерация HTML для отображения карт в WebView

#### `YandexGeocodingService`
**Назначение:** Геокодирование через Yandex API

**Основные методы:**
- `GetCoordinatesAsync(string address)` - адрес → координаты
- `GetAddressAsync(double latitude, double longitude)` - координаты → адрес

### Сервисы поиска и фильтрации

#### `ISearchService` / `SearchService`
**Назначение:** Поиск и фильтрация событий

**Основные методы:**
- Поиск по тексту
- Фильтрация по категориям
- Фильтрация по дате

#### `FilterStateService`
**Назначение:** Управление состоянием активных фильтров

**Свойства:**
- `SearchText` - текст поиска
- `SelectedCategory` - выбранная категория
- `SelectedDate` - выбранная дата
- `SelectedInterests` - выбранные интересы
- `HasActiveFilters` - наличие активных фильтров
- `ActiveFilterLabels` - метки активных фильтров

**Методы:**
- `ClearFilters()` - очистка фильтров

**События:**
- `FiltersChanged` - изменение фильтров

### Сервисы навигации

#### `INavigationService` / `NavigationService`
**Назначение:** Навигация между страницами

**Основные методы:**
- `NavigateToAsync()` - навигация к странице
- `GoBackAsync()` - возврат назад

#### `NavigationStateService`
**Назначение:** Управление состоянием навигации

### Сервисы модерации

#### `IReportService` / `ReportService`
**Назначение:** Работа с жалобами на события

**Основные методы:**
- `CreateReportAsync()` - создание жалобы
- `GetReportsAsync()` - получение жалоб
- `ResolveReportAsync()` - разрешение жалобы

#### `IAuthorizationService` / `AuthorizationService`
**Назначение:** Проверка прав доступа

**Основные методы:**
- Проверка роли пользователя
- Проверка прав на редактирование

### Сервисы состояния

#### `MapViewStateService`
**Назначение:** Управление состоянием отображения карты

**Свойства:**
- `IsMapViewActive` - активен ли режим карты

#### `CreateEventStateService`
**Назначение:** Сохранение состояния при создании события

### Дополнительные сервисы

#### `UserProfileService`
**Назначение:** Работа с профилями пользователей

#### `LocationSelectionService`
**Назначение:** Выбор местоположения для события

---

## ViewModels и их функции

### `BaseViewModel`
**Базовый класс** для всех ViewModels

**Функции:**
- Реализация `INotifyPropertyChanged`
- Метод `OnPropertyChanged()` для уведомления об изменениях
- Метод `SetProperty()` для установки свойств с уведомлением

### `HomeViewModel`
**Назначение:** Главная страница приложения

**Основные функции:**
- Загрузка списка событий
- Переключение между списком и картой
- Поиск событий
- Применение фильтров
- Присоединение к событиям
- Навигация к созданию события

**Свойства:**
- `Events` - список событий
- `IsMapView` - режим отображения (список/карта)
- `SearchText` - текст поиска
- `IsLoading` - состояние загрузки

### `EventDetailsViewModel`
**Назначение:** Детальная информация о событии

**Основные функции:**
- Отображение деталей события
- Присоединение/выход из события
- Редактирование события (для создателя)
- Удаление события (для создателя)
- Подача жалобы на событие

**Свойства:**
- `Event` - текущее событие
- `CanJoin` - можно ли присоединиться
- `CanEdit` - можно ли редактировать

### `CreateEventViewModel`
**Назначение:** Создание нового события

**Основные функции:**
- Валидация данных события
- Выбор категории
- Выбор местоположения (вручную или на карте)
- Сохранение события

**Свойства:**
- `Title` - название события
- `Description` - описание
- `SelectedCategory` - выбранная категория
- `Address` - адрес
- `EventDate` - дата события
- `MaxParticipants` - максимальное количество участников

### `ProfileViewModel`
**Назначение:** Профиль пользователя

**Основные функции:**
- Отображение информации о пользователе
- Редактирование профиля
- Отображение статистики (созданные события, участие)
- Навигация к настройкам

**Свойства:**
- `User` - данные пользователя
- `DisplayName` - имя пользователя
- `Email` - email
- `City` - город
- `About` - о пользователе
- `SelectedInterests` - выбранные интересы

### `SettingsViewModel`
**Назначение:** Настройки приложения

**Основные функции:**
- Изменение языка
- Изменение темы
- Изменение пароля
- Настройка двухфакторной аутентификации
- Управление уведомлениями
- Очистка кэша
- Удаление аккаунта

### `MyEventsViewModel`
**Назначение:** События пользователя

**Основные функции:**
- Отображение созданных событий
- Отображение событий, в которых пользователь участвует
- Отображение архивных событий
- Переключение между вкладками

**Свойства:**
- `CreatedEvents` - созданные события
- `ParticipatingEvents` - события участия
- `ArchivedEvents` - архивные события
- `SelectedTab` - выбранная вкладка

### `FilterViewModel`
**Назначение:** Фильтрация событий

**Основные функции:**
- Поиск по тексту
- Фильтрация по категории
- Фильтрация по дате
- Фильтрация по интересам
- Сброс фильтров

### `SearchViewModel`
**Назначение:** Поиск событий

**Основные функции:**
- Поиск по тексту
- Отображение результатов поиска

### `ModeratorDashboardViewModel`
**Назначение:** Панель модератора

**Основные функции:**
- Просмотр жалоб
- Разрешение жалоб
- Блокировка событий

### `ReportsManagementViewModel`
**Назначение:** Управление жалобами

**Основные функции:**
- Просмотр всех жалоб
- Фильтрация жалоб по статусу
- Разрешение жалоб

### `SelectInterestsViewModel`
**Назначение:** Выбор интересов пользователя

**Основные функции:**
- Отображение доступных интересов
- Выбор/снятие выбора интересов
- Сохранение выбранных интересов

### `MapLocationPickerViewModel`
**Назначение:** Выбор местоположения на карте

**Основные функции:**
- Отображение карты
- Выбор координат
- Получение адреса по координатам

### `AuthViewModel`
**Назначение:** Аутентификация

**Основные функции:**
- Вход в систему
- Регистрация
- Валидация данных входа

---

## Модели данных

### `Event`
**Модель события**

**Основные свойства:**
- `Id` - уникальный идентификатор
- `Title` - название события
- `Description` - описание
- `CategoryId` - категория (устаревшее)
- `CategoryIds` - список категорий
- `Address` - адрес
- `Latitude`, `Longitude` - координаты
- `EventDate` - дата и время события
- `CreatedAt` - дата создания
- `CreatorId` - ID создателя
- `CreatorName` - имя создателя
- `MaxParticipants` - максимальное количество участников
- `ParticipantIds` - список ID участников
- `IsActive` - активно ли событие
- `IsBlocked` - заблокировано ли событие
- `BlockedBy`, `BlockedAt`, `BlockReason` - информация о блокировке

**Вычисляемые свойства:**
- `ParticipantsCount` - количество участников
- `HasFreeSpots` - есть ли свободные места
- `IsFull` - заполнено ли событие
- `IsRelevant` - актуально ли событие
- `IsCompleted` - завершено ли событие
- `DateDisplay` - форматированная дата
- `CategoriesDisplay` - отображение категорий

### `User`
**Модель пользователя**

**Основные свойства:**
- `Id` - уникальный идентификатор
- `DisplayName` - отображаемое имя
- `Email` - email
- `Role` - роль (enum: User, Organizer, Moderator, Admin)
- `IsActive` - активен ли пользователь
- `CreatedAt` - дата создания
- `BlockedUntil` - дата разблокировки
- `InterestIds` - список ID интересов
- `City` - город
- `About` - о пользователе
- `AvatarUrl` - URL аватара
- `UpdatedAt` - дата обновления
- `CreatedEventsCount` - количество созданных событий
- `ParticipatedEventsCount` - количество событий участия

### `Interest`
**Модель интереса**

**Основные свойства:**
- `Id` - уникальный идентификатор
- `Name` - название интереса
- `IsSelected` - выбран ли интерес (для UI)

### `Report`
**Модель жалобы**

**Основные свойства:**
- `Id` - уникальный идентификатор
- `TargetEventId` - ID события, на которое подана жалоба
- `ReporterUserId` - ID пользователя, подавшего жалобу
- `Type` - тип жалобы (enum: Spam, Inappropriate, Scam, Illegal, Other)
- `Reason` - причина жалобы
- `Status` - статус (enum: Pending, Approved, Rejected)
- `CreatedAt` - дата создания
- `ResolvedBy` - ID модератора, разрешившего жалобу
- `ResolvedAt` - дата разрешения
- `ModeratorNotes` - заметки модератора

### `AuditLog`
**Модель лога аудита**

**Основные свойства:**
- `Id` - уникальный идентификатор
- `Action` - действие
- `UserId` - ID пользователя
- `TargetId` - ID цели действия
- `Timestamp` - время действия
- `Details` - детали действия

### `MapEvent`
**Модель события для карты**

**Основные свойства:**
- Свойства события для отображения на карте
- Координаты для маркера

---

## Дополнительная информация

### Обработка ошибок

Приложение использует try-catch блоки для обработки ошибок и логирование через `System.Diagnostics.Debug.WriteLine()`.

### Асинхронность

Все операции с данными выполняются асинхронно через `async/await` паттерн.

### Кэширование

Состояние фильтров и навигации кэшируется в State Services для сохранения между переходами.

### Безопасность

- Аутентификация через Firebase Authentication
- Проверка прав доступа через `AuthorizationService`
- Валидация данных на стороне клиента и сервера

---

## Контакты и поддержка

Для вопросов и предложений создайте issue в репозитории проекта.

