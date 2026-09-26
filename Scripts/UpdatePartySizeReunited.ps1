[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ChangeNotes,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^v?\d+\.\d+\.\d+$')]
    [string]$GameVersion
)

$ErrorActionPreference = "Stop"

$itemId = "3372837208"
$moduleName = "PartySizeReunited"
$gameVersionTag = if ($GameVersion.StartsWith("v")) { $GameVersion } else { "v$GameVersion" }
$tags = @("Enhancement", "Utility", "Other", "Singleplayer", $gameVersionTag)

if ([string]::IsNullOrWhiteSpace($env:BANNERLORD_GAME_DIR)) {
    throw "La variable d'environnement BANNERLORD_GAME_DIR n'est pas definie."
}

$gameDirectory = $env:BANNERLORD_GAME_DIR.TrimEnd('\', '/')
$workshopExecutable = Join-Path $gameDirectory "bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.SteamWorkshop.exe"
$moduleDirectory = if ([string]::IsNullOrWhiteSpace($env:PARTY_SIZE_REUNITED_MODULE_DIR)) {
    Join-Path $gameDirectory "Modules\$moduleName"
} else {
    $env:PARTY_SIZE_REUNITED_MODULE_DIR.TrimEnd('\', '/')
}

foreach ($requiredPath in @($workshopExecutable, $moduleDirectory)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Chemin introuvable : $requiredPath"
    }
}

$temporaryXmlPath = Join-Path ([System.IO.Path]::GetTempPath()) "$moduleName-$([Guid]::NewGuid()).xml"

try {
    $document = New-Object System.Xml.XmlDocument
    $tasks = $document.AppendChild($document.CreateElement("Tasks"))

    $getItem = $tasks.AppendChild($document.CreateElement("GetItem"))
    $itemIdElement = $getItem.AppendChild($document.CreateElement("ItemId"))
    $itemIdElement.SetAttribute("Value", $itemId)

    $updateItem = $tasks.AppendChild($document.CreateElement("UpdateItem"))
    $moduleFolderElement = $updateItem.AppendChild($document.CreateElement("ModuleFolder"))
    $moduleFolderElement.SetAttribute("Value", $moduleDirectory)
    $changeNotesElement = $updateItem.AppendChild($document.CreateElement("ChangeNotes"))
    $changeNotesElement.SetAttribute("Value", $ChangeNotes)

    $tagsElement = $updateItem.AppendChild($document.CreateElement("Tags"))
    foreach ($tag in $tags) {
        $tagElement = $tagsElement.AppendChild($document.CreateElement("Tag"))
        $tagElement.SetAttribute("Value", $tag)
    }

    $writerSettings = New-Object System.Xml.XmlWriterSettings
    $writerSettings.Indent = $true
    $writerSettings.Encoding = New-Object System.Text.UTF8Encoding($false)
    # The TaleWorlds tool reads xmlDocument.FirstChild as the Tasks element.
    $writerSettings.OmitXmlDeclaration = $true
    $writer = [System.Xml.XmlWriter]::Create($temporaryXmlPath, $writerSettings)
    try {
        $document.Save($writer)
    } finally {
        $writer.Dispose()
    }

    Push-Location (Split-Path -Parent $workshopExecutable)
    try {
        $uploaderLogPath = Join-Path (Get-Location) "steam_workshop_uploader.txt"
        $uploadStartedAtUtc = [DateTime]::UtcNow
        & $workshopExecutable $temporaryXmlPath
        $workshopExitCode = $LASTEXITCODE

        if (-not (Test-Path -LiteralPath $uploaderLogPath -PathType Leaf) -or
            (Get-Item -LiteralPath $uploaderLogPath).LastWriteTimeUtc -lt $uploadStartedAtUtc.AddSeconds(-2)) {
            throw "Le journal Steam Workshop n'a pas ete cree ou actualise. Code : $workshopExitCode."
        }

        $uploaderOutput = Get-Content -LiteralPath $uploaderLogPath -Raw
        if ($uploaderOutput -notmatch '(?m)^Uploading done!\r?$') {
            throw "La mise a jour Steam Workshop n'a pas ete confirmee. Code : $workshopExitCode.`n$uploaderOutput"
        }

        if ($workshopExitCode -ne 0) {
            Write-Warning "Steam a confirme l'upload, mais son attente finale Console.ReadKey a echoue (code $workshopExitCode)."
        }
    } finally {
        Pop-Location
    }
} finally {
    Remove-Item -LiteralPath $temporaryXmlPath -Force -ErrorAction SilentlyContinue
}

Write-Host "Mise a jour Steam Workshop terminee."
