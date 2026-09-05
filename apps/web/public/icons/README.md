# App icons

The manifest references three icons that **do not exist yet**:

| File | Size | Purpose |
| --- | --- | --- |
| `icon-192.png` | 192×192 | Standard launcher icon |
| `icon-512.png` | 512×512 | Splash screen and high-DPI launchers |
| `icon-maskable-512.png` | 512×512 | Adaptive icon |

**TODO:** produce these from the ZEE logo once it exists.

The maskable one is not optional. Android crops adaptive icons to whatever shape the
launcher uses (circle, squircle, rounded square), so keep all meaningful content
inside the centre 80% "safe zone" — otherwise the logo gets its edges sliced off on
most phones. Verify with https://maskable.app before committing.

Until these are added, the PWA installs with a browser-generated fallback icon.
