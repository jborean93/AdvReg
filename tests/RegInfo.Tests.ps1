. ([IO.Path]::Combine($PSScriptRoot, 'common.ps1'))

Describe "Get-RegInfo" {
    BeforeEach {
        $hkcuPrefix = "\REGISTRY\USER\$([System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value)"

        Get-Item -Path HKCU:\MyTest -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
        New-Item -Path HKCU:\MyTest | Out-Null
    }

    AfterEach {
        Get-Item -Path HKCU:\MyTest -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
    }

    It "Gets normal key info with -Path" {
        $actual = Get-RegInfo -Path HKCU:\MyTest
        $actual -is [AdvReg.KeyInformation] | Should -Be $true
        $actual.PSPath | Should -Be "Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\MyTest"
        $actual.NTPath | Should -Be "$hkcuPrefix\MyTest"
        $actual.LastWriteTime -is [DateTime] | Should -Be $true
        $actual.LastWriteTime.Kind | Should -Be "Utc"
        $actual.TitleIndex | Should -Be 0
        $actual.Name | Should -Be "MyTest"
        $actual.Class | Should -Be ""
        $actual.SubKeys | Should -Be 0
        $actual.ValueCount | Should -Be 0
        $actual.UserFlags | Should -Be ([AdvReg.KeyUserFlags]::None)
        $actual.ControlFlags | Should -Be ([AdvReg.KeyControlFlags]::None)
        $actual.Target | Should -Be $null
        $actual.VirtualizationCandidate | Should -Be $false
        $actual.VirtualizationEnabled | Should -Be $false
        $actual.VirtualTarget | Should -Be $false
        $actual.VirtualStore | Should -Be $false
        $actual.VirtualSource | Should -Be $false
        $actual.HandleTags -is [UInt32] | Should -Be $true
        $actual.TrustedKey | Should -Be $false
    }

    It "Gets key info with -LiteralPath" {
        New-Item -Path HKCU:\MyTest\Fancy[name]
        $actual = Get-RegInfo -LiteralPath HKCU:\MyTest\Fancy[name]
        $actual -is [AdvReg.KeyInformation] | Should -Be $true
        $actual.PSPath | Should -Be "Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\MyTest\Fancy[name]"
        $actual.NTPath | Should -Be "$hkcuPrefix\MyTest\Fancy[name]"
        $actual.LastWriteTime -is [DateTime] | Should -Be $true
        $actual.LastWriteTime.Kind | Should -Be "Utc"
        $actual.TitleIndex | Should -Be 0
        $actual.Name | Should -Be "Fancy[name]"
        $actual.Class | Should -Be ""
        $actual.SubKeys | Should -Be 0
        $actual.ValueCount | Should -Be 0
        $actual.UserFlags | Should -Be ([AdvReg.KeyUserFlags]::None)
        $actual.ControlFlags | Should -Be ([AdvReg.KeyControlFlags]::None)
        $actual.Target | Should -Be $null
        $actual.VirtualizationCandidate | Should -Be $false
        $actual.VirtualizationEnabled | Should -Be $false
        $actual.VirtualTarget | Should -Be $false
        $actual.VirtualStore | Should -Be $false
        $actual.VirtualSource | Should -Be $false
        $actual.HandleTags -is [UInt32] | Should -Be $true
        $actual.TrustedKey | Should -Be $false
    }

    It "Gets key info" {
        New-RegLink -Path HKCU:\MyTest\Link -Target HKCU:\MyTest
        try {
            $actual = Get-RegInfo -LiteralPath HKCU:\MyTest\Link
            $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Link) | Should -Be $true
            $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Volatile) | Should -Be $false
            $actual.Target | Should -Be "$hkcuPrefix\MyTest"
        }
        finally {
            Remove-RegLink -Path HKCU:\MyTest\Link
        }
    }

    It "Gets volatile key info" {
        New-RegLink -Path HKCU:\MyTest\Link -Target HKCU:\MyTest -Volatile
        try {
            $actual = Get-RegInfo -LiteralPath HKCU:\MyTest\Link
            $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Link) | Should -Be $true
            $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Volatile) | Should -Be $true
            $actual.Target | Should -Be "$hkcuPrefix\MyTest"
        }
        finally {
            Remove-RegLink -Path HKCU:\MyTest\Link
        }
    }

    It "Gets control flags information" {
        New-Item -Path HKLM:\SOFTWARE\AdvRegTest | Out-Null
        try {
            reg.exe FLAGS HKLM\Software\AdvRegTest SET DONT_VIRTUALIZE RECURSE_FLAG

            $actual = Get-RegInfo -LiteralPath HKLM:\SOFTWARE\AdvRegTest
            $actual.ControlFlags | Should -Be ([AdvReg.KeyControlFlags]"DontVirtualize, RecurseFlag")

            reg.exe FLAGS HKLM\Software\AdvRegTest SET DONT_SILENT_FAIL

            $actual = Get-RegInfo -LiteralPath HKLM:\SOFTWARE\AdvRegTest
            $actual.ControlFlags | Should -Be ([AdvReg.KeyControlFlags]::DontSilentFail)
        }
        finally {
            Remove-Item -Path HKLM:\SOFTWARE\AdvRegTest -Force -Recurse
        }
    }
}
