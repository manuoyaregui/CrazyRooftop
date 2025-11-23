# Procedural City Generator

Sistema de generación procedural de ciudades para CrazyRooftop parkour game.

## 🚀 Quick Start

### 1. Crear el Config Asset
1. En Unity, navega a `Assets/Project/City/Configs/`
2. Click derecho en el panel Project
3. `Create > CrazyRooftop > City > Generator Config`
4. Nómbralo "DefaultCityConfig" (o el nombre que prefieras)
5. Configura los parámetros (ver sección "Configurar Parámetros" abajo)

### 2. Setup en Escena
1. Crea un GameObject vacío en la escena
2. Nombra el GameObject "CityGenerator"
3. Añade el componente `CityGenerator`
4. Arrastra tu config asset al campo "Config"
5. Presiona Play - ¡La ciudad se generará automáticamente!

### 3. Configurar Parámetros

Abre tu config asset y ajusta:

**Street Layout**
- `Grid Size X/Z`: Número de bloques (default: 8x8)
- `Passage Width`: Ancho de pasadizos entre edificios (30 unidades)
- `Street Width Min/Max`: Ancho de calles principales (50-60 unidades)

**Grid Distortion**
- `Distortion Intensity`: 0 = grid perfecta, 1 = muy orgánico (default: 0.3)
- `Noise Scale`: Escala del ruido Perlin (default: 0.1)
- `Max Block Rotation`: Rotación máxima de bloques en grados (default: 15°)

**Building Sizes**
- `Building Size X`: Ancho (80-120 unidades)
- `Building Size Y`: Altura (300-500 unidades) - ¡Muy altos para parkour!
- `Building Size Z`: Profundidad (80-120 unidades)

**Building Spacing**
- `Min/Max Building Spacing`: Distancia entre edificios (20-35 unidades)
- `Buildings Per Block`: Cuántos edificios por bloque (default: 3)

**Visual**
- `Building Material`: Material para edificios (opcional)
- `Show Debug Gizmos`: Muestra calles y bloques en Scene view

### 4. Regenerar Ciudad

**En Play Mode:**
- La ciudad se regenera automáticamente al entrar en Play

**En Editor:**
- Click derecho en el componente `CityGenerator`
- Selecciona "Generate City" del menú contextual
- O usa "Clear City" para limpiar

**Cambiar Seed:**
- Cambia el `seed` en el config para generar una ciudad diferente
- Mismo seed = misma ciudad (determinístico)

## 🎮 Integración con Gameplay

### Spawn del Jugador
```csharp
CityGenerator cityGen = FindObjectOfType<CityGenerator>();
GameObject randomBuilding = cityGen.GetRandomBuilding();
// Coloca al jugador en el techo del edificio
```

### Obtener Todos los Edificios
```csharp
List<GameObject> buildings = cityGen.GetBuildings();
foreach (GameObject building in buildings)
{
    BuildingData data = building.GetComponent<BuildingData>();
    Debug.Log($"Building {data.buildingId} - Size: {data.size}");
}
```

## 🔧 Debug & Visualization

**Scene View Gizmos:**
- 🟡 Amarillo: Intersecciones de calles
- 🔵 Cyan: Calles principales
- 🟢 Verde: Pasadizos
- 🟠 Naranja: Límites de bloques
- 🟢 Verde claro: Área usable de bloques

## 📋 Parámetros Recomendados

**Para Parkour Intenso:**
- Building Spacing: 20-25 unidades
- Buildings Per Block: 4-5
- Distortion: 0.2-0.3 (más predecible)

**Para Ciudad Orgánica:**
- Distortion: 0.4-0.6
- Max Block Rotation: 20-30°
- Noise Scale: 0.05-0.15

**Para Ciudad Densa:**
- Grid Size: 12x12 o mayor
- Buildings Per Block: 5-8
- Building Spacing: 20-25

## 🔮 Futuro: Generación Infinita

El sistema está preparado para generación infinita con chunks:
- `Chunk Size` ya está en el config (500 unidades)
- Generación basada en seed (determinística)
- Arquitectura modular lista para streaming

## 📁 Estructura de Archivos

```
Assets/Project/City/
├── Scripts/
│   ├── CityGenerator.cs          (Manager principal)
│   ├── CityGeneratorConfig.cs    (ScriptableObject)
│   ├── StreetLayoutGenerator.cs  (Genera calles)
│   ├── BlockGenerator.cs         (Genera bloques)
│   ├── BuildingPlacer.cs         (Coloca edificios)
│   └── BuildingData.cs           (Componente de edificio)
├── Configs/
│   └── DefaultCityConfig.asset   (Config por defecto)
└── Materials/
    └── BuildingMaterial.mat      (Material básico)
```
