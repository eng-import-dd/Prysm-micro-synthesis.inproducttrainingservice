[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True,Position=1)]
    [string]$SourceFolder
)

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\Constants\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\Controllers\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\Entity\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\EventHandlers\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\Events\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\Models\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\Modules\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\Requests\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\Responses\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules\Validators\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules.Test\Modules\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules.Test\Validators\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$folder = ($PSScriptRoot + "\Synthesis.InProductTraining.Modules.Test\Workflow\")
if ((test-path $folder)) {
    Remove-Item -Recurse -Force $folder
    Write-Output "DELETED $folder"
}

$directories = Get-ChildItem "$SourceFolder\Synthesis.InProductTraining.Modules\" | where {$_.Attributes -match 'Directory'}
ForEach ($d in $directories) 
{
    switch ($d.Name) 
    {
      "bin" {}
      "obj" {}
      "Owin" {}
      "Properties" {}
      "x64" {}
      "x86" {}
      default 
      {
        $folder = ("$SourceFolder\Synthesis.InProductTraining.Modules\" + $d.Name)
        Copy-Item -Path $folder -Recurse -Destination ($PSScriptRoot + "\Synthesis.InProductTraining.Modules") -Container
        Write-Output "COPIED: $folder"
      }
    }
}

$directories = Get-ChildItem "$SourceFolder\Synthesis.InProductTraining.Modules.Test\" | where {$_.Attributes -match 'Directory'}
ForEach ($d in $directories) 
{
    switch ($d.Name) 
    {
      "bin" {}
      "obj" {}
      "Properties" {}
      default 
      {
        $folder = ("$SourceFolder\Synthesis.InProductTraining.Modules.Test\" + $d.Name)
        Copy-Item -Path $folder -Recurse -Destination ($PSScriptRoot + "\Synthesis.InProductTraining.Modules.Test") -Container
        Write-Output "COPIED: $folder"
      }
    }
}