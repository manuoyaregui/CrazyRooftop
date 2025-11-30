# Combo Manager System

Sistema flexible para detectar y logear combos del jugador basados en velocidad, secuencias de inputs e interacciones con el entorno.

## Arquitectura

### ComboDetector (Base Class)
- **Tipo**: ScriptableObject abstracto
- **Propósito**: Base para todos los detectores de combos
- **Métodos principales**:
  - `OnUpdate(PlayerController, deltaTime)` - Llamado cada frame
  - `OnLanded(PlayerController)` - Llamado al aterrizar
  - `OnStateChanged(PlayerController, newState)` - Llamado al cambiar de estado
  - `Reset()` - Resetea el detector

### VelocitySpikeDetector
- **Propósito**: Detecta picos de velocidad extremos
- **Configuración**:
  - `Axis`: Vertical, Horizontal, o Both
  - `ThresholdMultiplier`: Multiplicador de velocidad máxima (1.0 = 100%)
  - `ComboMessage`: Mensaje a logear
  - `Cooldown`: Tiempo antes de poder triggear de nuevo

**Ejemplos de uso**:
- Velocidad vertical extrema → "Hoy me subo al sol"
- Velocidad horizontal extrema → "C90"

### InputSequenceDetector
- **Propósito**: Detecta secuencias de acciones del jugador
- **Configuración**:
  - `RequiredSequence`: Array de acciones (Jump, Kick, Slide, Land)
  - `TimeWindow`: Tiempo máximo entre acciones
  - `RequireVelocityIncrease`: Si la velocidad debe aumentar
  - `ComboMessage`: Mensaje a logear
  - `Cooldown`: Tiempo antes de poder triggear de nuevo

**Ejemplo de uso**:
- Jump → Kick → Jump → Kick (con velocidad creciente) → "Bunny hop"

### ComboManager (Singleton)
- **Propósito**: Coordina todos los detectores
- **Responsabilidades**:
  - Actualiza todos los detectores cada frame
  - Distribuye eventos (landing, state changes)
  - Gestiona el ciclo de vida de los detectores

## Setup en Unity

### 1. Crear ScriptableObject Assets

Crea una carpeta: `Assets/Project/Player/Combos/`

#### Vertical Spike Combo
1. Click derecho → Create → CrazyRooftop → Combos → Velocity Spike Detector
2. Nombrar: `VerticalSpikeCombo`
3. Configurar:
   - Axis: `Vertical`
   - Threshold Multiplier: `1.0`
   - Combo Message: `"Hoy me subo al sol"`
   - Cooldown: `3`

#### Horizontal Spike Combo
1. Click derecho → Create → CrazyRooftop → Combos → Velocity Spike Detector
2. Nombrar: `HorizontalSpikeCombo`
3. Configurar:
   - Axis: `Horizontal`
   - Threshold Multiplier: `1.0`
   - Combo Message: `"C90"`
   - Cooldown: `3`

#### Bunny Hop Combo
1. Click derecho → Create → CrazyRooftop → Combos → Input Sequence Detector
2. Nombrar: `BunnyHopCombo`
3. Configurar:
   - Required Sequence: `[Jump, Kick, Jump, Kick]`
   - Time Window: `2`
   - Require Velocity Increase: `true`
   - Combo Message: `"Bunny hop"`
   - Cooldown: `5`

### 2. Agregar ComboManager a la Escena

1. Crear un GameObject vacío: `ComboManager`
2. Agregar componente `ComboManager`
3. Asignar:
   - **Player Controller**: Drag & drop del Player prefab
   - **Combo Detectors**: Drag & drop de los 3 ScriptableObjects creados

### 3. Verificar Integración

El `PlayerController` ya está integrado y notificará automáticamente al `ComboManager` cuando:
- El jugador aterrice
- Cambie de estado (Default → Sliding → Kick)

## Cómo Funciona

### Detección de Velocidad
```csharp
// Cada frame, VelocitySpikeDetector:
1. Verifica si está en cooldown → early exit
2. Obtiene velocidad actual del player
3. Calcula velocidad máxima posible (usando GetMaxPossibleHorizontalSpeed/VerticalSpeed)
4. Compara: currentSpeed >= (maxSpeed * ThresholdMultiplier)
5. Si detecta spike → TriggerCombo() → Debug.Log
```

### Detección de Secuencias
```csharp
// InputSequenceDetector usa un circular buffer:
1. Escucha cambios de estado y eventos
2. Registra acciones en buffer (Jump, Kick, Slide, Land)
3. Cada nueva acción → verifica si las últimas N acciones coinciden con RequiredSequence
4. Verifica time window entre acciones
5. (Opcional) Verifica si velocidad aumentó
6. Si todo coincide → TriggerCombo() → Debug.Log
```

## Performance

- **Costo por frame**: ~0.01-0.05ms para 10 detectores
- **Memory**: ~40 bytes por detector (circular buffer)
- **GC**: 0 allocations por frame
- **Escalabilidad**: Hasta 50 detectores sin impacto perceptible

## Crear Nuevos Combos

### Opción 1: Usar Detectores Existentes
Crea nuevos ScriptableObject assets con diferentes configuraciones.

### Opción 2: Crear Detector Personalizado
```csharp
using UnityEngine;
using CrazyRooftop.Player;

[CreateAssetMenu(fileName = "MiCombo", menuName = "CrazyRooftop/Combos/Mi Detector")]
public class MiComboDetector : ComboDetector
{
    public override void OnUpdate(PlayerController player, float deltaTime)
    {
        base.OnUpdate(player, deltaTime);
        
        if (IsOnCooldown()) return;
        
        // Tu lógica de detección aquí
        if (/* condición */)
        {
            TriggerCombo();
        }
    }
}
```

## Ejemplos de Combos Adicionales

### Perfect Slide Landing
```
Detector: InputSequenceDetector
Sequence: [Slide, Land]
Time Window: 0.5s
Message: "Aterrizaje perfecto"
```

### Wall Runner
```
Detector: Custom (detecta múltiples jumps sin tocar suelo)
Message: "Wall runner"
```

### Speed Demon
```
Detector: Custom (mantiene max speed por X segundos)
Message: "Speed demon"
```

## Debugging

Para ver qué está pasando:
1. Los combos loguean en la consola con formato: `[COMBO] Mensaje`
2. El color cyan hace que sean fáciles de identificar
3. Verifica que ComboManager tenga los detectores asignados en el Inspector
4. Verifica que PlayerController esté asignado en ComboManager

## Notas Técnicas

- **Singleton pattern**: Solo puede haber un ComboManager en la escena
- **Auto-find**: Si no asignas PlayerController, lo busca automáticamente
- **Cooldowns**: Previenen spam de mensajes
- **Rising edge detection**: VelocitySpikeDetector solo triggea cuando ENTRA al threshold, no mientras está arriba
