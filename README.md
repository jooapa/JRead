# JRead

**JRead** is the ultimate cross-platform C# console input library for .NET 8+

---

## Usage

```csharp
string Read(string startText = "", string? preText = null, JReadOptions? options = null);
string? ReadNull(string startText = "", string? preText = null, JReadOptions? options = null);
string Read(string? startText = null, JReadOptions? options = null);
string? ReadNull(string? startText = null, JReadOptions? options = null);
```

### Simple Example
```csharp
using JRead;

// Basic input
string name = JRead.Read("Enter your name: ");
Console.WriteLine($"Hello, {name}!");

// Input with prefilled text
string edited = JRead.Read("Edit this: ", "DefaultValue");
Console.WriteLine($"Final value: {edited}");
```

### Advanced Example
```csharp
using JRead;

var options = new JReadOptions
{
    EnableMaskedInput = true,           // Hide password input
    MaskedInputChar = '*',              // Use * for masking
    EnableAutoComplete = true,          // Enable tab completion
    AutoCompleteItems = new List<string> { "admin", "user", "guest" },
    AutoCompleteMinLength = 2,          // Show suggestions after 2 chars
    AddReturnedValueToHistory = false,  // Don't save passwords to history
    EscapingReturnsTheOriginalInput = false // ESC returns null
};

string? password = JRead.ReadNull("Password: ", options: options);
if (password == null)
{
    Console.WriteLine("Login cancelled!");
    return;
}
```

## JReadOptions Reference

**`CustomHistory`** (`JReadHistory?`, default: `null`)  
Custom history instance instead of global history

**`EnableDebug`** (`bool`, default: `false`)  
Print debug information for key presses

**`AddReturnedValueToHistory`** (`bool`, default: `true`)  
Add the returned value to command history

**`EscapingReturnsTheOriginalInput`** (`bool`, default: `true`)  
If true, ESC returns original input; if false, returns null

**`AutoCompleteItems`** (`List<string>`, default: `[]`)  
List of autocomplete suggestions

**`EnableAutoComplete`** (`bool`, default: `true`)  
Enable tab-completion functionality

**`AutoCompleteMinLength`** (`int`, default: `1`)  
Minimum characters before showing suggestions

**`AutoCompleteCaseSensitive`** (`bool`, default: `false`)  
Case-sensitive autocomplete matching

**`EnableMaskedInput`** (`bool`, default: `false`)  
Mask input characters (for passwords)

**`MaskedInputChar`** (`char`, default: `'*'`)  
Character used for masking input

**`NewLineOnExit`** (`bool`, default: `true`)  
Add newline when input completes

**`MaxDisplayLength`** (`int?`, default: `null`)  
Maximum characters to display (windowed view)

**`SubtractFromAvailableSpace`** (`bool`, default: `false`)  
Subtract MaxDisplayLength from console width

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Arrow Keys` | Move cursor left/right |
| `Ctrl + Left/Right` | Move by word |
| `Home/End` | Move to start/end of line |
| `Backspace/Delete` | Delete characters |
| `Ctrl + W` | Delete word to the left |
| `Ctrl + U/Z` | Undo last action |
| `Ctrl + Y` | Redo last undone action |
| `Up/Down Arrows` | Navigate command history |
| `Tab` | Autocomplete (if enabled) |
| `Enter` | Submit input |
| `Escape` | Cancel or return original (based on options) |

## Features

- **Line Editing:** Insert, delete, overwrite text anywhere in the input line
- **Cursor Movement:** Move cursor by character, word, or to start/end of line  
- **Autocompletion:** Tab-complete words with customizable suggestions
- **Undo/Redo:** Full undo/redo support with Ctrl+U/Z and Ctrl+Y
- **Masked Input:** Hide input for passwords or sensitive data
- **Command History:** Navigate through previous inputs with Up/Down arrows
- **Terminal Resize Support:** Handles terminal resizing gracefully
- **Cross-Platform:** Works on Windows, Linux, and macOS
- **Customizable Options:** Extensive configuration through JReadOptions