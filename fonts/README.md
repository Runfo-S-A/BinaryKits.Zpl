# Fuente combinada

`Unified-CondensedBold.ttf` combina los caracteres disponibles en:

- `ATTriumvirate-CondensedBold.ttf`: todos sus caracteres, con prioridad sobre las demás fuentes (latino, español, inglés, números y símbolos).
- `ANMDS.ttf`: escrituras asiáticas y puntuación CJK/ancho completo disponibles en el original.
- `AssistB.ttf`: caracteres de escritura hebrea.
- `ARIALNB.TTF`: caracteres de escritura cirílica.

Los caracteres ausentes en los originales no se crean. Los símbolos de otras fuentes fuera de estas selecciones no se incorporan. Se normalizan las fuentes a 2.048 unidades por em y se eliminan las instrucciones de hinting para evitar mezclar programas de fuentes distintas. Se conservan las tablas de composición y posicionamiento que procesa FontTools.

Para regenerar desde la raíz del repositorio:

```powershell
python -m pip install fonttools==4.67.0
python fonts/merge_fonts.py
```

El script comprueba la cobertura exacta, las métricas de cada carácter y textos de muestra en los cuatro grupos. Los originales permanecen intactos. La integración con el visor y la validación visual/en impresora se realizan por separado; el orden bidireccional del hebreo depende del motor de texto.

Se conservan los avisos de copyright y licencia presentes en las fuentes. La combinación no otorga permisos adicionales de modificación o distribución: deben revisarse las licencias originales antes de distribuirla.
