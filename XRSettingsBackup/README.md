# Archived duplicate OpenXR settings

`OpenXRPackageSettings.asset` and its `.meta` file were moved here unchanged
from `Assets/XR/Settings/` to fix the Android build failure in OpenXR 1.16.1
(`MetaQuestFeatureBuildHooks.ApplySettingsOverride`, line 41).

The active settings are `Assets/XR/Settings/OpenXR Package Settings.asset`
(GUID `d12e303ac568c274fb67ef69ecf03afc`), referenced by
`ProjectSettings/EditorBuildSettings.asset`. The archived asset's GUID,
`a9a6963505ddf7f4d886008c6dc86122`, had no references in the scanned project
assets, scenes, prefabs, scripts, package configuration, or project settings.

Both assets contained a `MetaQuestFeature`. The package's build hook searches
for all assets containing this type and returns null unless exactly one asset
is found, then dereferences the result without checking it.

Keep this backup outside `Assets` so Unity does not import it. To recover an old
setting, compare it with the active asset and transfer the needed value rather
than restoring a second OpenXR settings asset into `Assets`.

After Unity refreshes its Asset Database, retry the Android build. The repair
was checked on disk; a successful full Android build has not been verified.
