. ([IO.Path]::Combine($PSScriptRoot, 'common.ps1'))

Describe "New-RegLink" {
    BeforeEach {
        $hkuPrefix = "\REGISTRY\USER"
        $hkcuPrefix = "$hkuPrefix\$([System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value)"
        $hklmPrefix = "\REGISTRY\MACHINE"

        Get-RegInfo -LiteralPath HKCU:\MyLink -ErrorAction SilentlyContinue | Remove-RegLink
        Get-RegInfo -LiteralPath HKLM:\SOFTWARE\MyLink -ErrorAction SilentlyContinue | Remove-RegLink
    }

    AfterEach {
        Get-RegInfo -LiteralPath HKCU:\MyLink -ErrorAction SilentlyContinue | Remove-RegLink
        Get-RegInfo -LiteralPath HKLM:\SOFTWARE\MyLink -ErrorAction SilentlyContinue | Remove-RegLink
    }

    It "Creates a link in the user hive" {
        New-RegLink -Path HKCU:\MyLink -Target HKCU:\Console
        $actual = Get-RegInfo -LiteralPath HKCU:\MyLink
        $actual.Name | Should -Be MyLink
        $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Link) | Should -Be $true
        $actual.Target | Should -Be "$hkcuPrefix\Console"
    }

    It "Creates a link in the machine hive" {
        New-RegLink -Path HKLM:\SOFTWARE\MyLink -Target HKLM:\SYSTEM
        $actual = Get-RegInfo -LiteralPath HKLM:\SOFTWARE\MyLink
        $actual.Name | Should -Be MyLink
        $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Link) | Should -Be $true
        $actual.Target | Should -Be "$hklmPrefix\SYSTEM"
    }

    It "Creates a volatile reg link" {
        New-RegLink -Path HKCU:\MyLink -Target HKCU:\Console -Volatile
        $actual = Get-RegInfo -LiteralPath HKCU:\MyLink
        $actual.Name | Should -Be MyLink
        $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Link) | Should -Be $true
        $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Volatile) | Should -Be $true
        $actual.Target | Should -Be "$hkcuPrefix\Console"
    }

    It "Creates a link with PSPath syntax" {
        New-RegLink -Path Registry::HKEY_CURRENT_USER\MyLink -Target Registry::HKEY_CURRENT_USER\Console
        $actual = Get-RegInfo -LiteralPath HKCU:\MyLink
        $actual.Name | Should -Be MyLink
        $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Link) | Should -Be $true
        $actual.Target | Should -Be "$hkcuPrefix\Console"
    }

    It "Create a link that crosses hives with a warning" {
        $warn = $null
        New-RegLink -Path HKCU:\MyLink -Target Registry::HKEY_USERS\.DEFAULT -WarningVariable warn
        $actual = Get-RegInfo -LiteralPath HKCU:\MyLink
        $actual.Name | Should -Be MyLink
        $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Link) | Should -Be $true
        $actual.Target | Should -Be "$hkuPrefix\.DEFAULT"
        $warn.Message | Should -Be "Link hive target must be in the same hive as the source to be valid."
    }

    It "Creates a link using the NT Target Path" {
        New-RegLink -Path HKCU:\MyLink -Target "$hkcuPrefix\Console"
        $actual = Get-RegInfo -LiteralPath HKCU:\MyLink
        $actual.Name | Should -Be MyLink
        $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Link) | Should -Be $true
        $actual.Target | Should -Be "$hkcuPrefix\Console"
    }

    It "Creates a link to a non-existant target" {
        New-RegLink -Path HKCU:\MyLink -Target "$hkcuPrefix\Fake\Target"
        $actual = Get-RegInfo -LiteralPath HKCU:\MyLink
        $actual.Name | Should -Be MyLink
        $actual.UserFlags.HasFlag([AdvReg.KeyUserFlags]::Link) | Should -Be $true
        $actual.Target | Should -Be "$hkcuPrefix\Fake\Target"
    }

    It "Fails to create link without parent" {
        $err = $null
        New-RegLink -Path HKCU:\\Missing\MyLink -Target "$hkcuPrefix\Console" -ErrorVariable err -ErrorAction SilentlyContinue
        [string]$err | Should -Be "Parent registry key HKEY_CURRENT_USER\Missing is missing and must be present to create a link."
    }
}

Describe "Remove-RegLink" {
    BeforeEach {
        $hkuPrefix = "\REGISTRY\USER"
        $hkcuPrefix = "$hkuPrefix\$([System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value)"

        Get-RegInfo -LiteralPath HKCU:\MyLink -ErrorAction SilentlyContinue | Remove-RegLink
    }

    AfterEach {
        Get-RegInfo -LiteralPath HKCU:\MyLink -ErrorAction SilentlyContinue | Remove-RegLink
    }

    It "Removes reg link using -Path" {
        New-RegLink -Path HKCU:\MyLink -Target HKCU:\Console
        Remove-RegLink -Path HKCU:\MyLink
        Test-Path -Path HKCU:\MyLink | Should -Be $false
        Test-Path -Path HKCU:\Console | Should -Be $true
    }

    It "Removed reg link using -LiteralPath" {
        New-RegLink -Path HKCU:\MyLink[test] -Target HKCU:\Console
        Remove-RegLink -LiteralPath HKCU:\MyLink[test]
        Test-Path -LiteralPath HKCU:\MyLink[test] | Should -Be $false
        Test-Path -Path HKCU:\Console | Should -Be $true
    }

    It "Removes missing target reg link" {
        New-RegLink -Path HKCU:\MyLink -Target HKCU:\Missing
        Remove-RegLink -LiteralPath HKCU:\MyLink
        Test-Path -LiteralPath HKCU:\MyLink | Should -Be $false
    }
}
