# Image Crop Region Feature — Implementation Plan

## Overview

Users select a 25:32 region on an uploaded image that defines the printable area. The crop region is stored as normalized coordinates (0–1) and applied purely via CSS in all display contexts — catalog, print HTML, and preview. No server-side image manipulation is needed.

---

## Step-by-Step Plan

### Step 1: Add DB fields for crop region

**File:** `MiniatureEntity.cs` (Domain/Entities/)

Add three new properties:
- `float CropXOffset` — top-left X of crop region, normalized 0–1
- `float CropYOffset` — top-left Y of crop region, normalized 0–1
- `float CropZoom` — zoom multiplier of the crop viewport (>= 1)

Update `MiniatureEntityConfiguration` if needed.

Create and apply EF Core migration:
```bash
dotnet ef migrations add AddCropRegionToMiniatures --project InitiativeTracker/InitiativeTracker.csproj
dotnet ef database update --project InitiativeTracker/InitiativeTracker.csproj
```

### Step 2: Update DTOs

**File:** `MiniatureCreateDto.cs` (Application/Dtos/)

Add optional crop fields:
```csharp
float? CropXOffset,
float? CropYOffset,
float? CropZoom,
```

**File:** `MiniatureUpdateDto.cs` (Application/Dtos/)

Add the same optional crop fields.

### Step 3: Update MiniatureService

**File:** `MiniatureService.cs` (Application/)

Pass crop fields through in `AddAsync`/`UpdateAsync`. In `UpdateAsync`, handle partial updates (nullable fields).

### Step 4: JS module for draggable/resizable crop region

**File:** `wwwroot/js/imageCrop.js` (new)

Browser-side logic for the interactive crop overlay:
- Renders a semi-transparent dimming overlay + a movable, resizable viewport rectangle over the preview image
- Drag to pan, wheel/slider to zoom
- Constrain aspect ratio to 25:32 at all times
- Clamp viewport within image boundaries
- On change, invoke Blazor callback via JSInterop to update normalized coordinates

### Step 5: Create Blazor crop picker component

**Files:** `ImageCropPicker.razor` + `.razor.cs` + `.razor.css` (new, under Components/)

Reusable component:
- Parameters: `string? ImageDataSrc`, two-way bindings for `CropXOffset`, `CropYOffset`, `CropZoom`
- In `OnAfterRenderAsync`: inject JS module, call init function passing image element reference + current crop state
- On every crop change, call a `[JSInvokable]` method that updates the bound parameters and triggers `StateHasChanged`

### Step 6: Integrate into AddMiniatureForm.razor

**File:** `AddMiniatureForm.razor`

Replace the plain `<img>` preview (lines 85–88) with the new `<ImageCropPicker>` component when an image is present (both create and edit flows).

In `OnParametersSet`, load existing crop region from `EditMiniature`. If no crop exists, initialize to defaults: center of image + minimum zoom that fills the 25:32 viewport.

Pass crop coordinates into `MiniatureCreateDto`/`MiniatureUpdateDto` in `OnSave`.

### Step 7: Apply CSS crop to MiniatureCatalog.razor

**File:** `MiniatureCatalog.razor`

Wrap each thumbnail `<img>` with a container that enforces the crop via CSS:
```html
<div class="crop-container">
  <img src="@imgSrc" style="--cx:@CropX; --cy:@CropY; --cz:@CropZoom;" ... />
</div>
```

CSS uses custom properties or inline computed transforms to position/scale the image. Thumbnail container keeps a 25:32 fixed ratio.

### Step 8: Apply CSS crop to MiniaturePrintGenerator.cs

**File:** `MiniaturePrintGenerator.cs` (Application/PrintHtmlGenerators/)

Update `MiniaturePrintDataDto` record to include crop fields.

In HTML generation, wrap each `<img>` with a positioning container div that applies the same CSS technique as Step 7 — absolute-positioned image scaled by `transform: scale(@CropZoom)` and translated by normalized offsets. Replace the current `object-fit: cover` approach since it doesn't respect user-chosen crop regions.

### Step 9: Tests, build, verify

Update existing unit tests to account for new DTO/entity fields. Run:
```bash
dotnet build
dotnet test
dotnet run --project InitiativeTracker/InitiativeTracker.csproj
```

---

## Default Crop Calculation

When no crop region exists (new miniatures or legacy data), compute a sensible default:

1. Determine the image's aspect ratio `R_img = imgWidth / imgHeight`
2. Target ratio is `R_target = 25/32 ≈ 0.78125`
3. If `R_img > R_target`: fit height, crop horizontally → zoom based on width ratio, center offset X
4. If `R_img <= R_target`: fit width, crop vertically → zoom based on height ratio, center offset Y
5. This ensures the viewport fills at least one axis without exceeding image boundaries
