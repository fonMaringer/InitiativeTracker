# План реализации InitiativeTracker v2

## Обзор текущего состояния
Проект на .NET 10 / Blazor Server, архитектура по слоям:
- `Domain/` — доменные модели (`InitiativeListItem`, `Source`)
- `Application/` — сервисы бизнес-логики (`InitiativeService`)
- `Infrastructure/Extensions/` — DI-регистрации и конфигурации
- `Components/Pages/`, `Components/Layout/` — UI (Blazor)
- `Integration/RestClients/` — HTTP-клиенты (ttg.club API)

Текущее хранилище инициативы: JSON-файл (`InitiativeList.json`).

---

## Этап 1 — Миграция на SQLite

### 1.1 Добавление пакетов NuGet
```
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### 1.2 Доменные модели (Domain/)
Создать EF Core entities:
- `InitiativeEntity.cs` — сущность инициативы (ID, Name, Initiative, Dexterity, HitsDefault, HitsCurrent, ArmorClass, ArmorClassCurrent, Link, SourceId, OrderIndex)
- `MiniatureEntity.cs` — миниатюра (ID, Name, ImagePath, Size, CroppedRegion_X, CroppedRegion_Y, CroppedRegion_Width, CroppedRegion_Height)
- `ItemEntity.cs` — предмет (ID, Name, Rarity, RequiresAttunement, Description)
- `SpellEntity.cs` — заклинание (ID, Name, VerbalComponent, SomaticComponent, MaterialComponent, Class, Description)

Перечисления:
- `CreatureSize.cs` — Tiny, Small, Medium, Large, Huge, Gargantuan
- `ItemRarity.cs` — Common, Uncommon, Rare, VeryRare, Legendary, Relic, Undefined, Varies
- `SpellClass.cs` — Cleric, Paladin, Wizard, Bard, Sorcerer, Warlock, Monk, Fighter, Barbarian, Artificer, Druid, Ranger

### 1.3 Контекст БД (Infrastructure/Database/)
Создать папку `Infrastructure/Database/`:
- `InitiativeTrackerDbContext.cs` — контекст EF Core с DbSet для всех сущностей
- `DbInitializer.cs` — сервис инициализации БД при старте (Database.EnsureCreated или миграции)

### 1.4 DI регистрация (Infrastructure/Extensions/DiExtensions.cs)
Добавить в `AddApplication()`:
```csharp
services.AddEntityFrameworkSqlite();
services.AddSingleton<InitiativeTrackerDbContext>();
```

### 1.5 Изменение InitiativeService
- Добавить конструктор с зависимостью от `InitiativeTrackerDbContext`
- `WarmUp()` → загрузка из БД вместо JSON
- `SaveToFile()` → сохранение в БД вместо JSON
- Удалить работу с файлом `InitiativeList.json`

### 1.6 Настройка подключения (appsettings.json)
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=initiativetracker.db"
  }
}
```

---

## Этап 2 — Печать миниатюр

### 2.1 Сервис (Application/MiniatureService.cs)
Интерфейс `IMiniatureService`:
- `Task AddAsync(MiniatureCreateDto dto)`
- `Task UpdateAsync(int id, MiniatureUpdateDto dto)`
- `Task DeleteAsync(int id)`
- `Task<MiniatureEntity?> GetByIdAsync(int id)`
- `Task<IReadOnlyList<MiniatureEntity>> SearchAsync(string query)`
- `Task<byte[]> GetImageAsync(int miniatureId)`

### 2.2 DTO модели (Application/Dtos/)
- `MiniatureCreateDto.cs` — Name, Size (enum), ImageData (byte[]), CropRegion { X, Y, Width, Height }
- `MiniaturePrintItemDto.cs` — MiniatureId, Quantity

### 2.3 Компоненты UI (Components/Pages/Miniatures/)
- `Miniatures.razor` — главная страница раздела миниатюр с табами/навигацией
- `AddMiniatureForm.razor` — форма добавления:
  - текстовое поле Name
  - выпадающий список Size
  - компонент загрузки изображения (InputFile)
  - предпросмотр с выбором области печати (кастомный Blazor-компонент или JS interop)
- `MiniatureCatalog.razor` — каталог существующих миниатюр
- `PreparationList.razor` — окно подготовки списка для печати:
  - поиск по названию (`SearchAsync`)
  - поле количества для каждой выбранной миниатюры
  - кнопка "Сформировать HTML для печати"

### 2.4 Генерация HTML (Application/PrintHtmlGenerators/)
- `MiniaturePrintGenerator.cs` — генерирует HTML со стилями для печати:
  - Группировка по размеру существа
  - Размеры: Tiny=16x12.5mm, Small=32x25mm, Medium=32x25mm, Large=64x50mm, Huge=96x75mm, Gargantuan=128x100mm
  - Два изображения одинакового размера, одно над другим, верхнее перевёрнуто по горизонти и вертикали
  - Рамка 1-2px вокруг каждого изображения

### 2.5 Хранение изображений (Infrastructure/Storage/)
- `IImageStorage.cs` — интерфейс для сохранения/загрузки изображений
- `FileSystemImageStorage.cs` — реализация, сохраняет файлы в `wwwroot/images/miniatures/`

---

## Этап 3 — Печать карточек предметов

### 3.1 Сервис (Application/ItemService.cs)
Интерфейс `IItemService`:
- `Task AddAsync(ItemCreateDto dto)`
- `Task UpdateAsync(int id, ItemUpdateDto dto)`
- `Task DeleteAsync(int id)`
- `Task<ItemEntity?> GetByIdAsync(int id)`
- `Task<IReadOnlyList<ItemEntity>> SearchAsync(string query)`

### 3.2 DTO модели (Application/Dtos/)
- `ItemCreateDto.cs` — Name, Rarity (enum), RequiresAttunement (bool), Description (HTML)

### 3.3 Компоненты UI (Components/Pages/Items/)
- `Items.razor` — главная страница раздела предметов
- `AddItemForm.razor` / `EditItemForm.razor` — форма добавления/редактирования:
  - текстовое поле Name
  - выпадающий список Rarity
  - чекбокс RequiresAttunement
  - WYSIWYG редактор для Description (интеграция с BlazorRichTextEdit или Quill.js через JS interop)
- `ItemPreparationList.razor` — подготовка печати:
  - поиск по названию
  - поле количества
  - кнопка генерации HTML

### 3.4 Генерация HTML (Application/PrintHtmlGenerators/)
- `ItemPrintGenerator.cs` — формат покерной карты (2.5" x 3.5"):
  - скруглённые углы, внутренний отступ
  - Название (жирное, крупнее)
  - Редкость + требуется настройка (меньше шрифт)
  - Описание (HTML из WYSIWYG, занимает оставшееся пространство)

---

## Этап 4 — Печать карточек заклинаний

### 4.1 Сервис (Application/SpellService.cs)
Интерфейс `ISpellService`:
- `Task AddAsync(SpellCreateDto dto)`
- `Task UpdateAsync(int id, SpellUpdateDto dto)`
- `Task DeleteAsync(int id)`
- `Task<SpellEntity?> GetByIdAsync(int id)`
- `Task<IReadOnlyList<SpellEntity>> SearchAsync(string query)`

### 4.2 DTO модели (Application/Dtos/)
- `SpellCreateDto.cs` — Name, VerbalComponent (bool), SomaticComponent (bool), MaterialComponent (bool), Class (enum), Description (HTML)

### 4.3 Компоненты UI (Components/Pages/Spells/)
- `Spells.razor` — главная страница раздела заклинаний
- `AddSpellForm.razor` / `EditSpellForm.razor` — форма добавления/редактирования:
  - текстовое поле Name
  - три чекбокса для компонентов (В/Ж/Р)
  - выпадающий список Class
  - WYSIWYG редактор для Description
- `SpellPreparationList.razor` — подготовка печати

### 4.4 Генерация HTML (Application/PrintHtmlGenerators/)
- `SpellPrintGenerator.cs` — формат покерной карты (2.5" x 3.5"):
  - скруглённые углы, внутренний отступ
  - Название (жирное, крупнее)
  - Компоненты В/Ж/Р как иконки или символы
  - Описание (HTML из WYSIWYG)
  - Класс (жирное, средний шрифт) внизу

---

## Этап 5 — Навигация и интеграция

### 5.1 Обновление Layout
- `Components/Layout/MainLayout.razor` — добавить пункты меню:
  Инициатива | Миниатюры | Предметы | Заклинания

### 5.2 Общие компоненты (Components/Shared/)
- `SearchAutocomplete.razor` — универсальный компонент автокомпита для поиска
- `PrintPreparationList.razor` — обёртка для списков подготовки печати

---

## Порядок выполнения

| Приоритет | Задача | Оценка сложности |
|-----------|--------|-----------------|
| P0 | Этап 1: миграция на SQLite | средняя |
| P1 | Этап 2: миниатюры | высокая (работа с изображениями, crop) |
| P2 | Этап 3: карточки предметов | средняя |
| P3 | Этап 4: карточки заклинаний | средняя |
| P4 | Этап 5: навигация и общие компоненты | низкая |

### Зависимости между этапами
- Этапы 2/3/4 зависят от Этапа 1 (БД)
- Этап 5 можно выполнять параллельно, но удобнее после 2/3/4

---

## Технические детали генерации HTML для печати

### Миниатюры
```css
@media print {
  .miniature-tiny    { width: 16mm; height: 12.5mm; }
  .miniature-small   { width: 32mm; height: 25mm; }
  .miniature-medium  { width: 32mm; height: 25mm; }
  .miniature-large   { width: 64mm; height: 50mm; }
  .miniature-huge    { width: 96mm; height: 75mm; }
  .miniature-gargantuan { width: 128mm; height: 100mm; }
}
.miniature-cell { border: 1px solid #000; display: flex; flex-direction: column; }
.miniature-flip { transform: rotate(180deg); }
```

### Предметы и заклинания (покерные карты)
```css
.poker-card {
  width: 2.5in; height: 3.5in;
  border-radius: 8px; padding: 4px;
  page-break-inside: avoid;
}
.card-title    { font-weight: bold; font-size: 1.2em; }
.card-subtitle { font-size: 0.9em; }
.card-content  { flex: 1; overflow: hidden; }
```

---

## Структура файлов после реализации

```
InitiativeTracker/
├── Application/
│   ├── Dtos/
│   │   ├── MiniatureCreateDto.cs
│   │   ├── MiniaturePrintItemDto.cs
│   │   ├── ItemCreateDto.cs
│   │   └── SpellCreateDto.cs
│   ├── PrintHtmlGenerators/
│   │   ├── MiniaturePrintGenerator.cs
│   │   ├── ItemPrintGenerator.cs
│   │   └── SpellPrintGenerator.cs
│   ├── MiniatureService.cs
│   ├── ItemService.cs
│   ├── SpellService.cs
│   └── InitiativeService.cs (изменён)
├── Components/
│   ├── Pages/
│   │   ├── Miniatures/
│   │   │   ├── Miniatures.razor
│   │   │   ├── AddMiniatureForm.razor
│   │   │   └── MiniaturePreparationList.razor
│   │   ├── Items/
│   │   │   ├── Items.razor
│   │   │   ├── AddItemForm.razor
│   │   │   └── ItemPreparationList.razor
│   │   └── Spells/
│   │       ├── Spells.razor
│   │       ├── AddSpellForm.razor
│   │       └── SpellPreparationList.razor
│   └── Shared/
│       ├── SearchAutocomplete.razor
│       └── ImageCropPreview.razor
├── Domain/
│   ├── InitiativeListItem.cs
│   ├── InitiativeEntity.cs
│   ├── MiniatureEntity.cs
│   ├── ItemEntity.cs
│   ├── SpellEntity.cs
│   └── Enums/
│       ├── CreatureSize.cs
│       ├── ItemRarity.cs
│       └── SpellClass.cs
├── Infrastructure/
│   ├── Database/
│   │   └── InitiativeTrackerDbContext.cs
│   ├── Extensions/
│   │   └── DiExtensions.cs (изменён)
│   └── Storage/
│       ├── IImageStorage.cs
│       └── FileSystemImageStorage.cs
└── wwwroot/
    └── images/
        └── miniatures/
```
