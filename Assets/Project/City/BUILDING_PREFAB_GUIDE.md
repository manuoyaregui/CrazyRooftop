# Building Prefab Guidelines

Guía completa para crear prefabs de edificios compatibles con el sistema de generación procedural.

## 🎯 Conceptos Clave

### Escala y Proporciones - NUEVO SISTEMA ✨

**¡Construye edificios con proporciones naturales!**

El sistema ahora **mantiene las proporciones de tu prefab** automáticamente. Esto significa que puedes construir edificios con formas realistas (altos y delgados) y el sistema los escalará correctamente.

**Cómo funciona:**
1. Construyes tu edificio con proporciones naturales (ej: 1 ancho x 4 alto x 1 profundo)
2. El sistema calcula cuánto debe escalar para alcanzar la altura objetivo (300-500 unidades)
3. Aplica el mismo factor de escala en todas las direcciones → **mantiene tus proporciones**

### Ejemplo Práctico:

**Tu prefab:** 10 x 40 x 10 unidades (proporción 1:4:1)
- Altura objetivo del sistema: 400 unidades
- Factor de escala: 400 / 40 = 10x
- **Resultado final:** 100 x 400 x 100 unidades ✅ (mantiene la proporción 1:4:1)

**Ventaja:** ¡Construyes edificios que se ven como edificios desde el principio!

---

## 📐 Dimensiones y Proporciones

### Tamaños Generados por el Sistema

El sistema genera edificios con estos rangos (configurables):
- **Ancho (X)**: 80-120 unidades
- **Alto (Y)**: 300-500 unidades  
- **Profundidad (Z)**: 80-120 unidades

### Proporciones Recomendadas para Prefabs

**✨ Edificio Estándar 1:4:1 (RECOMENDADO)**
```
Prefab: 10 x 40 x 10 unidades
Proporción: 1:4:1 (ancho:alto:profundidad)
Resultado: ~100 x 400 x 100
```
👍 Perfecto para edificios normales, fácil de construir

**Rascacielos 1:6:1**
```
Prefab: 10 x 60 x 10 unidades
Proporción: 1:6:1
Resultado: ~83 x 500 x 83
```
👍 Edificios extra altos y delgados

**Edificio Ancho 1.5:4:1**
```
Prefab: 15 x 40 x 10 unidades
Proporción: 1.5:4:1
Resultado: ~150 x 400 x 100
```
👍 Edificios rectangulares en planta

**Torre Delgada 0.7:5:0.7**
```
Prefab: 7 x 50 x 7 unidades
Proporción: 0.7:5:0.7
Resultado: ~70 x 500 x 70
```
👍 Torres estrechas y muy altas

---

## 🏗️ Estructura del Prefab

### Jerarquía Recomendada

```
BuildingPrefab_01
├── Mesh (modelo 3D)
├── Collider (BoxCollider o MeshCollider)
├── Details (opcional)
│   ├── Windows
│   ├── Roof
│   └── Decorations
└── (BuildingData se añade automáticamente)
```

### Componentes Esenciales

#### 1. **Collider** (OBLIGATORIO)
- Añade un `BoxCollider` o `MeshCollider`
- Asegúrate de que cubra todo el edificio
- El jugador necesita poder pararse en el techo

```csharp
// El sistema espera que el collider esté en el root o en un hijo
BoxCollider collider = GetComponent<BoxCollider>();
```

#### 2. **Pivot Point** (IMPORTANTE)
- El pivot debe estar en el **centro inferior** del edificio
- Posición Y = 0 en la base del edificio
- Esto asegura que el edificio se coloque correctamente en el suelo

```
     Y
     ↑
     |
  [Edificio]
     |
     •────→ X  (Pivot aquí, en el centro de la base)
    /
   Z
```

#### 3. **Materiales**
- Usa materiales que se vean bien a diferentes escalas
- Considera usar texturas con tiling para ventanas repetidas
- Evita detalles muy pequeños que desaparezcan al escalar

---

## 🎮 Consideraciones de Parkour

### Distancias Entre Edificios

El sistema mantiene estas distancias:
- **Mínimo**: 20 unidades
- **Máximo**: 35 unidades

**Implicaciones para diseño:**
- El jugador debe poder saltar ~20-35 unidades
- Considera añadir bordes/cornisas en los techos para aterrizajes
- Evita geometría que bloquee saltos horizontales

### Techos Navegables

**Recomendaciones:**
- Techo plano o con pendiente suave
- Evita objetos altos en el centro del techo
- Si añades decoraciones (antenas, AC units), colócalas en los bordes

**Ejemplo de techo ideal:**
```
Vista superior del techo:
┌─────────────────┐
│  [AC]      [AC] │  ← Decoraciones en bordes
│                 │
│    ESPACIO      │  ← Centro libre para aterrizar
│    LIBRE        │
│  [AC]      [AC] │
└─────────────────┘
```

### Alturas y Pisos

Con edificios de 300-500 unidades de alto:
- **~3-5 "pisos" visuales** de 100 unidades cada uno
- Considera añadir detalles cada ~100 unidades (ventanas, balcones)
- Variación de altura ayuda a la navegación visual

---

## 🎨 Detalles Visuales

### Ventanas y Fachadas

**Opción 1: Textura con Tiling**
```csharp
// Material con textura de ventanas repetidas
material.mainTextureScale = new Vector2(10, 40); // 10 ventanas ancho, 40 alto
```

**Opción 2: Geometría Modular**
```
Crea un módulo de ventana de 1x1
Repítelo en el prefab
El sistema escalará todo proporcionalmente
```

### Variaciones Recomendadas

Crea múltiples prefabs con diferentes estilos:

**Set Básico (3-5 prefabs):**
1. Edificio cuadrado estándar (1:1:1)
2. Edificio delgado/torre (0.7:1:0.7)
3. Edificio ancho (1.5:1:1)
4. Rascacielos (1:2:1)
5. Edificio bajo/comercial (1:0.5:1)

**Set Avanzado (añade):**
- Edificios con formas en L o T
- Edificios con retranqueos (setbacks)
- Edificios con techos inclinados
- Edificios con antenas/estructuras en techo

---

## ⚙️ Setup en Unity

### Crear el Prefab

1. **Crear GameObject base**
   ```
   GameObject → 3D Object → Empty
   Nombre: "BuildingPrefab_01"
   ```

2. **Añadir modelo 3D**
   - Arrastra tu modelo como hijo
   - O crea geometría con ProBuilder/primitivas

3. **Configurar pivot**
   - Asegúrate de que el pivot esté en Y=0, centro de la base
   - Usa un GameObject vacío como root si es necesario

4. **Añadir Collider**
   ```
   Add Component → Box Collider
   Ajusta el tamaño para cubrir el edificio
   ```

5. **Crear Prefab**
   ```
   Arrastra el GameObject a Assets/Project/City/Prefabs/
   ```

### Asignar Prefabs al Config

1. Abre tu `CityGeneratorConfig`
2. En la sección **Visual**:
   - Expande **Building Prefabs**
   - Cambia el tamaño del array (ej: 5)
   - Arrastra tus prefabs a los slots

```
CityGeneratorConfig
└── Visual
    ├── Building Prefabs
    │   ├── Element 0: BuildingPrefab_01
    │   ├── Element 1: BuildingPrefab_02
    │   ├── Element 2: BuildingPrefab_03
    │   └── ...
    ├── Use Procedural Fallback: ✓
    └── Building Material: [tu material]
```

---

## ✅ Checklist de Validación

Antes de usar tu prefab, verifica:

- [ ] Pivot en el centro inferior (Y=0)
- [ ] Escala base ~1x1x1 unidades
- [ ] Tiene Collider
- [ ] Techo navegable (plano o con pendiente suave)
- [ ] Materiales se ven bien a diferentes escalas
- [ ] No tiene scripts que dependan de escala específica
- [ ] Funciona bien rotado (el sistema rota edificios)

---

## 🔧 Troubleshooting

### "Mi edificio aparece flotando o enterrado"
→ Revisa que el pivot esté en Y=0 en la base

### "Mi edificio se ve estirado/aplastado"
→ El sistema escala proporcionalmente. Ajusta las proporciones del prefab

### "Las texturas se ven mal al escalar"
→ Usa materiales con tiling o geometría en lugar de texturas detalladas

### "El jugador cae a través del techo"
→ Asegúrate de que el Collider cubra todo el edificio, incluyendo el techo

### "Los edificios se solapan"
→ El sistema usa el tamaño del prefab escalado para spacing. Asegúrate de que el collider sea preciso

---

## 💡 Tips Avanzados

### Variación de Color
```csharp
// Añade este script a tu prefab para variación de color
public class BuildingColorVariation : MonoBehaviour
{
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = Random.ColorHSV(0f, 1f, 0.3f, 0.7f, 0.5f, 1f);
    }
}
```

### LOD (Level of Detail)
```csharp
// Para ciudades grandes, considera añadir LOD Group
LODGroup lodGroup = gameObject.AddComponent<LODGroup>();
// Configura niveles de detalle
```

### Iluminación
- Añade Emission maps para ventanas iluminadas
- Considera baked lighting para mejor performance
- Usa Light Probes para iluminación dinámica

---

## 📊 Ejemplos de Configuración

### Ciudad Densa (Muchos edificios pequeños)
```
Building Size Y: 200-400
Buildings Per Block: 5-7
Building Spacing: 20-25
```

### Ciudad de Rascacielos
```
Building Size Y: 400-600
Buildings Per Block: 2-3
Building Spacing: 30-40
```

### Ciudad Mixta (Recomendado)
```
Building Size Y: 300-500
Buildings Per Block: 3-4
Building Spacing: 20-35
Prefabs: Mix de alturas y formas
```

---

## 🎯 Resumen Rápido

**Para empezar:**
1. Crea edificio con proporciones naturales (ej: 10x40x10 unidades = proporción 1:4:1)
2. Pivot en el centro de la base (Y=0)
3. Añade BoxCollider
4. Guarda como prefab
5. Asigna al CityGeneratorConfig
6. ¡Prueba en Play mode!

**Recuerda:**
- **Construye con proporciones realistas**: 1:4:1, 1:5:1, 1:6:1 (ancho:alto:profundidad)
- El sistema mantiene tus proporciones y escala basándose en la altura objetivo
- Techo debe ser navegable
- Distancias entre edificios: 20-35 unidades
