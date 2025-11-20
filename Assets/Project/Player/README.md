# Player System - Setup Guide

## 📦 Archivos Copiados

Se han copiado los scripts del ejemplo del KCC a tu proyecto:

1. **PlayerController.cs** - Lógica de movimiento del personaje
2. **Player.cs** - Manejo de inputs
3. **PlayerCamera.cs** - Sistema de cámara orbital

Todos usan el namespace `CrazyRooftop.Player`

---

## 🚀 Setup Rápido

### 1. Crear el GameObject del Player

1. Crea un GameObject vacío llamado "Player"
2. Agrega estos componentes:
   - `KinematicCharacterMotor` (del KCC)
   - `PlayerController` (nuestro script)
   - `Player` (nuestro script)
   - `CapsuleCollider` (radio: 0.5, altura: 2)

3. Crea un hijo del Player llamado "PlayerVisual":
   - Agrega un `Capsule` mesh
   - Escala: (1, 1, 1)

4. Crea otro hijo del Player llamado "CameraFollowPoint":
   - Position: (0, 1.5, 0)

### 2. Configurar el PlayerController

En el Inspector:
- **Motor**: Asigna el KinematicCharacterMotor
- **Mesh Root**: Asigna el "PlayerVisual"
- **Camera Follow Point**: Asigna el "CameraFollowPoint"

### 3. Crear la Cámara

1. Crea un GameObject vacío llamado "PlayerCamera"
2. Agrega estos componentes:
   - `PlayerCamera` (nuestro script)
   - `Camera` (componente de Unity)

3. En el componente PlayerCamera:
   - **Camera**: Asigna el componente Camera

### 4. Conectar Player con Cámara

En el componente `Player`:
- **Character**: Asigna el PlayerController
- **Character Camera**: Asigna el PlayerCamera

---

## 🎮 Controles

- **WASD**: Movimiento
- **Mouse**: Rotar cámara
- **Scroll**: Zoom
- **Click Derecho**: Toggle primera/tercera persona
- **Espacio**: Saltar
- **C**: Agacharse

---

## ⚙️ Parámetros Principales

### PlayerController
- **Max Stable Move Speed**: Velocidad máxima (default: 10)
- **Stable Movement Sharpness**: Aceleración (default: 15)
- **Jump Up Speed**: Fuerza del salto (default: 10)

### PlayerCamera
- **Default Distance**: Distancia de la cámara (default: 6)
- **Rotation Speed**: Sensibilidad del mouse (default: 1)

---

## 📝 Próximos Pasos

Ahora que tienes la base funcionando, puedes:
1. Ajustar los parámetros a tu gusto
2. Modificar los controles en `Player.cs`
3. Agregar nuevas mecánicas en `PlayerController.cs`
