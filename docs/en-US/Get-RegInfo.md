---
external help file: AdvReg.dll-Help.xml
Module Name: AdvReg
online version:
schema: 2.0.0
---

# Get-RegInfo

## SYNOPSIS
Get more detailed information about a registry key.

## SYNTAX

### Path (Default)
```
Get-RegInfo [-Path] <String[]> [<CommonParameters>]
```

### LiteralPath
```
Get-RegInfo -LiteralPath <String[]> [<CommonParameters>]
```

## DESCRIPTION
Used to gather more information about a registry key that is not exposed in the normal registry provider in PowerShell.
This includes details such as the NTPath of a key, whether it is volatile, a link, and other low level bits of information.

## EXAMPLES

### Example 1: Get information about a registry key
```
PS C:\> Get-RegInfo -Path HKLM:\SOFTWARE
```

Gets the details of the key at `HKEY_LOCAL_MACHINE\SOFTWARE`.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

The path(s) to get the link information for.

## OUTPUTS

### AdvReg.KeyInformation

## NOTES

## RELATED LINKS
