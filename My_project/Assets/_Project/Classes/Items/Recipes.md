Recipes.md - не удалять! имя файла для цитирования

## L0 — Raw Resources (Сырьё)
Сырьё не крафтится, используется как вход для L1.
- Structural Metals
- Heavy Metals
- Alloys Ore
- High-Tech Metals
- Precious Metals
- Reactive Metals
- Minerals
- Salts
- Inorganic Carbon
- Hydrocarbons
- Atmospheric Gases
- Reactive Gases
- Noble Gases
- Ice
- Radioactive Elements

## L1 — Refined Feedstock (1–2 входа) (из L0)
Рецепты L1 (фиксированные количества):
- Metal Stock (1x): 3x Structural Metals (L0) + 2x Heavy Metals (L0)
- Precision Alloy Stock (1x): 3x Alloys Ore (L0) + 3x High-Tech Metals (L0)
- Chemical Feedstock (1x): 3x Reactive Metals (L0) + 2x Salts (L0)
- Silicate Feedstock (1x): 3x Minerals (L0) + 2x Ice (L0)
- Carbon Feedstock (1x): 3x Inorganic Carbon (L0) + 2x Hydrocarbons (L0)
- Industrial Gases (1x): 2x Atmospheric Gases (L0) + 2x Reactive Gases (L0)
- Isotope Feedstock (1x): 2x Noble Gases (L0) + 2x Radioactive Elements (L0)

## L2 — Materials (2–3 входа) (из L1)
Рецепты L2 (фиксированные количества):
- Structural Composites (1x): 3x Metal Stock (L1) + 2x Silicate Feedstock (L1) + 2x Chemical Feedstock (L1)
- Ceramics (1x): 2x Silicate Feedstock (L1) + 2x Industrial Gases (L1) + 1x Precision Alloy Stock (L1) + 1x Chemical Feedstock (L1)
- Polymers (1x): 3x Carbon Feedstock (L1) + 2x Chemical Feedstock (L1) + 1x Industrial Gases (L1)
- Conductive Materials (1x): 2x Metal Stock (L1) + 3x Industrial Gases (L1) + 2x Isotope Feedstock (L1)
- Magnetic Materials (1x): 3x Metal Stock (L1) + 2x Precision Alloy Stock (L1) + 2x Chemical Feedstock (L1)
- Optical Materials (1x): 3x Silicate Feedstock (L1) + 2x Carbon Feedstock (L1) + 1x Isotope Feedstock (L1)
- Semiconductors (1x): 2x Carbon Feedstock (L1) + 2x Silicate Feedstock (L1) + 3x Isotope Feedstock (L1)
- Radiation Shielding Materials (1x): 3x Precision Alloy Stock (L1) + 2x Chemical Feedstock (L1)

## L3 — Functional Components (2–4 входа) (из L1 L2)
Рецепты L3 (фиксированные количества):
- 2x Power Controller:
    - 4x Structural Composites (L2)
    - 3x Semiconductors (L2)
    - 5x Conductive Materials (L2)
- 1x Capacitor Bank:
    - 3x Conductive Materials (L2)
    - 3x Polymers (L2)
    - 2x Magnetic Materials (L2)
    - 3x Carbon Feedstock (L1)
- 1x Heat Exchanger:
    - 3x Structural Composites (L2)
    - 2x Radiation Shielding Materials (L2)
    - 2x Polymers (L2)
    - 2x Industrial Gases (L1)
- 1x Nozzle Assembly:
    - 2x Structural Composites (L2)
    - 3x Magnetic Materials (L2)
    - 3x Ceramics (L2)
    - 3x Chemical Feedstock (L1)
- 1x Pump / Compressor:
    - 3x Optical Materials (L2)
    - 3x Ceramics (L2)
    - 2x Polymers (L2)
    - 2x Industrial Gases (L1)
- 1x Gyro / IMU:
    - 3x Magnetic Materials (L2)
    - 3x Semiconductors (L2)
    - 2x Conductive Materials (L2)
    - 3x Precision Alloy Stock (L1)
- 1x Sensor Array:
    - 3x Optical Materials (L2)
    - 3x Semiconductors (L2)
    - 2x Radiation Shielding Materials (L2)
    - 3x Isotope Feedstock (L1)
- 1x Guidance Computer:
    - 3x Semiconductors (L2)
    - 2x Conductive Materials (L2)
    - 3x Polymers (L2)
    - 2x Industrial Gases (L1)
- 1x Field Emitter:
    - 2x Structural Composites (L2)
    - 2x Magnetic Materials (L2)
    - 3x Optical Materials (L2)
    - 3x Carbon Feedstock (L1)
- 1x Armor Module:
    - 2x Structural Composites (L2)
    - 3x Radiation Shielding Materials (L2)
    - 3x Ceramics (L2)
    - 3x Metal Stock (L1)

## L4 — Subsystems (3–5 входа) (из L1 L2 L3)
Рецепты L4 (фиксированные количества):
- 1x Power Core:
    - 2x Power Controller (L3)
    - 1x Capacitor Bank (L3)
    - 1x Heat Exchanger (L3)
    - 1x Field Emitter (L3)
    - 2x Magnetic Materials (L2)
- 1x Propulsion Core:
    - 2x Gyro / IMU (L3)
    - 1x Nozzle Assembly (L3)
    - 1x Pump / Compressor (L3)
    - 3x Metal Stock (L1)
    - 2x Industrial Gases (L1)
- 1x Defense Core:
    - 1x Armor Module (L3)
    - 1x Field Emitter (L3)
    - 1x Power Controller (L3)
    - 1x Capacitor Bank (L3)
    - 2x Radiation Shielding Materials (L2)
- 1x Weapon Core:
    - 1x Guidance Computer (L3)
    - 1x Field Emitter (L3)
    - 1x Power Controller (L3)
    - 1x Gyro / IMU (L3)
    - 2x Conductive Materials (L2)
- 1x Sensor Core:
    - 2x Sensor Array (L3)
    - 1x Guidance Computer (L3)
    - 1x Capacitor Bank (L3)
    - 2x Semiconductors (L2)
    - 2x Carbon Feedstock (L1)
- 1x Engineering Core:
    - 1x Heat Exchanger (L3)
    - 1x Pump / Compressor (L3)
    - 1x Armor Module (L3)
    - 1x Nozzle Assembly (L3)
    - 2x Chemical Feedstock (L1)

## L5 — Equipment (4–6 входов) (из L4)
Финальные изделия (фиксированные количества):
### Питание
- 1x Reactor:
    - 2x Power Core (L4)
    - 1x Defense Core (L4)
    - 1x Engineering Core (L4)
    - 1x Sensor Core (L4)
- 1x Energy Buffer:
    - 2x Power Core (L4)
    - 1x Engineering Core (L4)
    - 1x Sensor Core (L4)
    - 1x Defense Core (L4)
### Движение
- 1x Main Thruster:
    - 2x Propulsion Core (L4)
    - 1x Power Core (L4)
    - 1x Engineering Core (L4)
    - 1x Defense Core (L4)
- 1x Maneuver Thruster:
    - 2x Propulsion Core (L4)
    - 1x Sensor Core (L4)
    - 1x Engineering Core (L4)
    - 1x Power Core (L4)
### Защита
- 1x Shield Generator:
    - 2x Defense Core (L4)
    - 1x Power Core (L4)
    - 1x Sensor Core (L4)
    - 1x Engineering Core (L4)
- 1x Directed Field Generator:
    - 2x Defense Core (L4)
    - 1x Weapon Core (L4)
    - 1x Power Core (L4)
    - 1x Sensor Core (L4)
### Оружие
- 1x Laser System:
    - 2x Weapon Core (L4)
    - 1x Propulsion Core (L4)
    - 1x Sensor Core (L4)
    - 1x Engineering Core (L4)
- 1x Gauss Gun:
    - 2x Weapon Core (L4)
    - 2x Propulsion Core (L4)
    - 1x Engineering Core (L4)
    - 1x Power Core (L4)
- 1x Kinetic Weapon:
    - 2x Weapon Core (L4)
    - 2x Propulsion Core (L4)
    - 1x Defense Core (L4)
    - 1x Power Core (L4)
- 1x Missile System:
    - 2x Weapon Core (L4)
    - 1x Sensor Core (L4)
    - 1x Engineering Core (L4)
    - 1x Power Core (L4)
### Сенсоры и управление
- 1x Sensor Suite:
    - 2x Sensor Core (L4)
    - 1x Power Core (L4)
    - 1x Engineering Core (L4)
    - 1x Defense Core (L4)
- 1x CPU:
    - 2x Sensor Core (L4)
    - 1x Propulsion Core (L4)
    - 1x Engineering Core (L4)
    - 1x Weapon Core (L4)
- 1x Targeting System:
    - 2x Sensor Core (L4)
    - 1x Weapon Core (L4)
    - 1x Power Core (L4)
    - 1x Engineering Core (L4)
### Инженерные
- 1x Heat Radiator:
    - 2x Engineering Core (L4)
    - 1x Propulsion Core (L4)
    - 1x Defense Core (L4)
    - 1x Sensor Core (L4)
- 1x Heat Dump System:
    - 2x Engineering Core (L4)
    - 1x Power Core (L4)
    - 1x Defense Core (L4)
    - 1x Weapon Core (L4)
- 1x Interdictor:
    - 2x Weapon Core (L4)
    - 1x Sensor Core (L4)
    - 1x Defense Core (L4)
    - 1x Power Core (L4)
- 1x Jammer:
    - 2x Sensor Core (L4)
    - 1x Weapon Core (L4)
    - 1x Defense Core (L4)
    - 1x Engineering Core (L4)
