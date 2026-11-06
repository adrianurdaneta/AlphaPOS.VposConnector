# Organización del repo AlphaPOS.VposConnector

Dejar el repositorio con un solo objetivo —el conector— y una estructura publicable en GitHub:
el material del proveedor y los archivos de scratch salen del árbol versionado sin perderse en disco.

## Specs

### S1 — Alcance del repo

> "El objetivo del repor es solo el conector. No interesa los archivos del sistema del tercero (Windows)."

El contenido versionado del repositorio es el conector, sus pruebas y su documentación. El runtime
del tercero (`Windows/`, 280 MB) no se versiona ni se publica; hoy ya está en `.gitignore`.

### S2 — Estructura destino

> "Habría que renombrar directorios y organizar correctamente el repo antes de publicarlo a Github."

```
src/AlphaPOS.VposConnector/            código, csproj, register_dll.bat, pubxml, README propio
tests/AlphaPOS.VposConnector.Tests/    pruebas y test.runsettings
docs/                                  manuales del conector, revisión y evidencia de certificación
samples/                               ejemplos VFP
README.md  CHANGELOG.md  .gitignore  AlphaPOS.VposConnector.slnx
```

### S3 — Material del proveedor fuera del árbol versionado

> "Fuera del repo, pero a salvo en disco"

Los 8 PDFs del proveedor, las 2 guías de integración VPOS y los 2 documentos de despliegue del
runtime Java se mueven a `_vendor/`, que queda ignorada. Ninguno está referenciado por la
documentación del conector. Siguen en disco y en el historial de git.

### S4 — Scratch fuera del árbol versionado

> "Mover a _scratch/ ignorada"

`example_from_vfp.txt` (una línea de log capturada), `prueba_vfp.prg` (impresora fiscal TFHKAIF,
ajeno al conector) y el `example_from_vfp.prg` de la raíz (desactualizado: la copia con la API de
dos argumentos vive junto al conector) se mueven a `_scratch/`, ignorada.

### S5 — Ninguna referencia rota

Las rutas del `.slnx`, el `ProjectReference` del proyecto de pruebas y todas las menciones a rutas
en los `.md` quedan apuntando a la estructura nueva. El build y la suite de pruebas siguen verdes.

### S6 — Directorio local

> "Renombrar a AlphaPOS.VposConnector"

La carpeta en disco pasa de `c:\dev\Vpos Universal 3.15.9 Certificada` a `c:\dev\AlphaPOS.VposConnector`,
que es el nombre del remoto y no tiene espacios que rompan scripts.

### S7 — Referencias ya roídas, corregidas

`CHANGELOG.md` nombra `CONTEXTO_VPOS_Y_PROYECTO.md` y `PLAN_ALINEAMIENTO_VPOS.md`, que no existen en
el repo, y cita el xlsx de certificación sin el sufijo ` (1)` del nombre real. Se corrigen al pasar.

### S8 — `.gitignore` completo

Se cubren `_vendor/`, `_scratch/`, `voucher/`, `.atl/` y `TestResults/`. Hoy `voucher/logs/vpos.log.1`
se escapa del patrón `*.log` y el directorio de runtime queda a la vista.

## Tasks

| ID | Specs | Ruta | Commit |
|---|---|---|---|
| T1 | S2, S3, S4, S5, S7, S8 | inline | `refactor(repo): dedicate the repository to the connector` |
| T2 | S5 | inline | verificación: build + suite |
| T3 | S6 | inline | rename del directorio local |

Riesgo: T1 mueve árboles versionados y puede romper el build (item 3: el `.slnx` y el
`ProjectReference` son contratos de compilación); T3 saca material versionado del árbol (recuperable
desde el historial). Por eso T4 verifica antes de cerrar.

## Log

- **L1** (pedido del propietario): "Ahora organiza VPOS. El objetivo del repor es solo el conector. No
  interesa los archivos del sistema del tercero (Windows). Habría que renombrar directorios y
  organizar correctamente el repo antes de publicarlo a Github."
- **L2** (decisiones del propietario): material del proveedor → "Fuera del repo, pero a salvo en
  disco" (`_vendor/` ignorada); archivos sueltos de la raíz → "Mover a _scratch/ ignorada";
  directorio local → "Renombrar a AlphaPOS.VposConnector".
- **L3** (contexto medido antes de tocar): 38 archivos versionados. Rompen al mover sólo
  `.slnx:2-3`, el `ProjectReference` de `AlphaPOS.VposConnector.Tests.csproj:12` y 9 menciones en
  docs. `register_dll.bat` se ubica con `%~dp0` y sobrevive intacto; `FolderProfile.pubxml:7` y
  `test.runsettings:5` son relativos al proyecto y también sobreviven. Ninguno de los 8 PDFs está
  referenciado por documentación versionada.
- **L4** (evidencia T1-T3, cierre): build **0 errores / 0 warnings** y suite **4/4** con
  `-p:PlatformTarget=x86`; el árbol versionado pasa de **38 a 23 archivos**. Tres hallazgos que
  cambiaron el plan:
  - **Desvío justificado**: el plan preveía tres commits (estructura / docs / untrack) y quedó uno
    solo. El `.slnx` y el `ProjectReference` sólo apuntan bien cuando el movimiento y su corrección
    están en el mismo commit; separarlos dejaría commits intermedios que no compilan.
  - **El primer `dotnet test` falló con `BadImageFormatException`**: no era la mudanza, era mi
    invocación sin `-p:PlatformTarget=x86`. El conector es x86 y el runner arrancaba en 64 bits. El
    README y los manuales ya lo documentaban; ahora lo dice también la sección de pruebas con la
    explicación del síntoma.
  - **El xlsx de certificación nunca estuvo trackeado** (siempre fue `??`), así que mi S7 lo suponía
    versionado sin serlo. Se movió con `mv` a `docs/Script_Certificacion_GrupoIDB_Vpos3.15.10.xlsx`
    (sin el sufijo ` (1)`) y se corrigió la referencia del CHANGELOG.
  - **`git mv` a `_vendor/` y `_scratch/` los deja trackeados**: hubo que sacarlos del índice con
    `git rm -r --cached`, que es lo que los vuelve invisibles para la publicación sin borrarlos del
    disco. Verificado con `git check-ignore`.
  - **CHANGELOG**: no se reescribieron las rutas de las entradas fechadas (son registro histórico);
    se agregó una sección `[Unreleased]` con la reorganización y la aclaración de que las entradas
    de abajo mencionan el layout anterior. Sí se eliminaron dos viñetas que nombraban archivos
    inexistentes.
  - **El xlsx de certificación se dejó fuera del índice**: nunca estuvo versionado y no se empieza a
    versionar un binario por iniciativa propia. Queda en disco en `docs/` y `.gitignore` cubre
    `docs/*.xlsx`. Si el propietario quiere versionarlo, se agrega con una línea.
- **L5** (evidencia T3, cierre): el directorio local pasó de `c:\dev\Vpos Universal 3.15.9 Certificada`
  a `c:\dev\AlphaPOS.VposConnector`. Verificado después del rename: build **0 avisos / 0 errores** y
  suite **4/4** desde la ruta nueva. `_vendor/` (27 MB) y `Windows/` (280 MB) siguen en disco y fuera
  del índice. Pendiente de decisión del propietario: publicar la rama `chore/repo-organization`
  (nace de `docs/vpos-connector-revision-20260827`, no de `master`), y si `master` se reemplaza o se
  mergea con PR.
- **L6** (defecto colateral encontrado y corregido): el rename del directorio rompió los **dos
  worktrees enlazados** (`adrianurdaneta-legendary-invention` y `adrianurdaneta-supreme-broccoli`).
  Su archivo `.git` guarda un puntero absoluto a
  `C:/dev/Vpos Universal 3.15.9 Certificada/.git/worktrees/...`, que dejó de existir. `git worktree
  list` desde la principal no lo delata, porque lee el registro de la principal y no el puntero de
  cada worktree. Se corrigió con `git worktree repair <ruta>...` y se verificó ejecutando `git
  branch`/`git status` dentro de cada uno: ambos vuelven a apuntar a
  `C:/dev/AlphaPOS.VposConnector/.git/worktrees/...`. Lección: renombrar la raíz de un repositorio
  con worktrees enlazados requiere `git worktree repair` y verificación dentro de cada worktree.
