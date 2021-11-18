---
external help file: AdvReg.dll-Help.xml
Module Name: AdvReg
online version:
schema: 2.0.0
---

# Remove-RegLink

## SYNOPSIS
Deletes a registry symlink.

## SYNTAX

### Path (Default)
```
Remove-RegLink [-Path] <String[]> [-WhatIf] [-Confirm] [<CommonParameters>]
```

### LiteralPath
```
Remove-RegLink -LiteralPath <String[]> [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Deletes a registry symlink at the path(s) specified.
This is used instead of `Remove-Item` as the latter will remove the target of the symlink rather than the link itself and also fails to remove broken symlinks.

## EXAMPLES

### Example 1: Delete a registry link
```
PS C:\> Remove-RegLink -Path HKLM:\SOFTWARE\MyLink
```

Removes the registry link at `HKEY_LOCAL_MACHINE\SOFTWARE\MyLink`.

## PARAMETERS

### -LiteralPath
Specifies a path to one or more locations.
The value of LiteralPath is used exactly as it is typed.
No characters are interpreted as wildcards.
If the path includes escape characters, enclose it in single quotation marks.
Single quotation marks tell PowerShell not to interpret any characters as escape sequences.

```yaml
Type: String[]
Parameter Sets: LiteralPath
Aliases: PSPath

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Path
Specifies a path of the registry link being removed.
Wildcard characters are permitted.
Using `-Path` will fail if the registry link points to an invalid target, use `-LiteralPath` instead for this scenario.

```yaml
Type: String[]
Parameter Sets: Path
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
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

The registry path link to remove.

## OUTPUTS

### None

## NOTES

Due to limitations in the registry provider in PowerShell using `-Path` may fail if the registry link at the path specified has an invalid target.
Using `-LiteralPath` bypasses this problem and can delete the link with an invalid target.

## RELATED LINKS
