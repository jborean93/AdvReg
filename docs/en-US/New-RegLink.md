---
external help file: AdvReg.dll-Help.xml
Module Name: AdvReg
online version:
schema: 2.0.0
---

# New-RegLink

## SYNOPSIS
Creates a registry symlink.

## SYNTAX

```
New-RegLink [-Path] <String[]> [-Target] <String> [-Volatile] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates a registry symlink at the path specified.

## EXAMPLES

### Example 1: Create symlink in HKCU hive
```
PS C:\> New-RegLink -Path HKCU:\MyLink -Target HKCU:\Console
```

This creates a registry symlink at `HKEY_CURRENT_USER\MyLink` that points to `HKEY_CURRENT_USER\Console`.

### Example 2: Create symlink using NT Path as target
```
PS C:\> New-RegLink -Path HKLM:\SYSTEM\MyLink -Target \REGISTRY\MACHINE\SOFTWARE\Microsoft
```

This creates a registry symlink at `HKEY_LOCAL_MACHINE\SYSTEM\MyLink` that points to `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft`.
Using the NT Path format `\REGISTRY\*` will skip the canonicalisation that the cmdlet runs to convert the Win32 registry path to the actual NT Path used in the symlink target.

## PARAMETERS

### -Path
The registry path where the symlink will be created.
This uses the same formats that the PowerShell registry provider supports, i.e. `HKLM:\...`, `Registry::HKEY_LOCAL_MACHINE\...`, etc.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Target
The target of the symlink.
This can either be a path in the formats the PowerShell registry provider supports.
Or it can be the full NT path that will be used as is.
The cmdlet will automatically convert the registry provider path to the NT path so the NT path should only be used when the the canonicalization is having problems.
The target must reside in the same registry hive as the symlink itself, the cmdlet will still continue to create the link with a warning but the link will not work.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Volatile
Creates a volatile symlink key.
A volatile key is removed once the hive is unloaded.
For the `HKLM` hive this is done on a reboot and for the user hives this is done when the profile is unloaded.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

The paths to create the link at.

### System.String

The target of the link(s) to create (by property name).

## OUTPUTS

### None

## NOTES

## RELATED LINKS
