# ??? Инструкции по тестированию вкладки модерации

## Проблема
Вкладка модерации ("??? Админ") не отображается на главной странице.

## Причина
Вкладка отображается только если:
1. ? Пользователь аутентифицирован (`IsAuthenticated == true`)
2. ? Пользователь имеет роль `Moderator` или `Admin` (`IsModeratorAsync() == true`)

## Решение

### Для тестирования используйте следующие ID пользователей:

#### 1. Администратор (всегда видит вкладку админ-панели)
- **ID**: `admin_test`
- **Название**: "Админ для тестирования"
- **Роль**: `Admin`

#### 2. Модератор (всегда видит вкладку админ-панели)
- **ID**: `test_moderator`
- **Название**: "Модератор для тестирования"
- **Роль**: `Admin` (на текущий момент в тестовых целях)

### Как использовать для тестирования:

#### Вариант 1: Прямая имитация через Debug

1. В `AuthStateService`, добавьте метод для тестирования:
```csharp
public void SetTestModerator(string userId)
{
    _testUserId = userId;
    _testIsAuthenticated = true;
    AuthenticationStateChanged?.Invoke(this, EventArgs.Empty);
}
```

2. Вызовите из кода тестирования:
```csharp
var authStateService = ServiceProvider.GetService<IAuthStateService>();
(authStateService as AuthStateService)?.SetTestModerator("admin_test");
```

#### Вариант 2: Через регистрацию/логин

1. Зарегистрируйте пользователя с email: `admin@test.com`
2. После логина система автоматически проверит роль пользователя
3. Если пользователь зарегистрирован с ID `admin_test`, вкладка появится

### Отладка через Debug Output

При запуске приложения в Debug режиме вы будете видеть в Output окне (Debug) следующие сообщения:

```
========== ?? AppShell инициализируется ==========
  - IAuthorizationService: ?
  - IAuthStateService: ?
? Обработчик AuthenticationStateChanged подписан
========== ?? Проверка прав модератора ==========
  - IsAuthenticated: True
  - CurrentUserId: admin_test
  - AuthorizationService != null: True
? Проверка прав завершена: модератор = True
??? AdminTab.IsVisible = True
========== ? Проверка прав завершена ==========
```

### Основной процесс:

1. **AppShell инициализируется** ? выводит информацию о сервисах
2. **Проверка аутентификации** ? проверяет `IsAuthenticated`
3. **Проверка прав** ? вызывает `IsModeratorAsync()`
4. **Получение роли пользователя** ? запрашивает роль из `IDataService.GetUserAsync()`
5. **Тестовая логика** ? если ID пользователя `admin_test` или `test_moderator`, устанавливает роль `Admin`
6. **Установка видимости** ? `AdminTab.IsVisible = isModerator`

### Возможные проблемы и решения:

| Проблема | Причина | Решение |
|----------|--------|--------|
| Вкладка не видна | Пользователь не аутентифицирован | Залогиньтесь |
| Вкладка не видна | У пользователя роль `User` | Используйте ID `admin_test` или `test_moderator` |
| Сервисы null | DI контейнер не инициализирован | Проверьте `MauiProgram.cs` регистрацию сервисов |
| Отсутствует `AdminTab` | Не найден элемент в `AppShell.xaml` | Проверьте, что `<Tab x:Name="AdminTab"` есть в файле |

### Для production-кода:

Замените тестовую логику в `FirestoreDataService.GetUserAsync()`:

```csharp
// ? УДАЛИТЕ этот код:
if (userId == "test_moderator" || userId == "admin_test")
{
    user.Role = UserRole.Admin;
}

// ? ИСПОЛЬЗУЙТЕ:
// Получайте роль из Firebase Firestore:
// var userDoc = await firestore.Collection("users").Document(userId).GetAsync();
// user.Role = (UserRole)userDoc.GetValue("role");
```

## Итоги

После выполнения этих инструкций:
- ? Вкладка модерации появится при логине пользователя с правами
- ? Debug Output покажет весь процесс инициализации
- ? Вы сможете тестировать функции админ-панели

Если вкладка всё еще не видна, проверьте Debug Output для точной диагностики проблемы.
