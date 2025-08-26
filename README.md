# JRead

**JRead** is the ultimate cross-platform C# console input library. For .NET 9

---

## Usage

### Basic Usage

```csharp
string Read(string? prefillText = null, string beginningText = "", JReadOptions? options = null)
string? ReadNull(string? prefillText = null, string beginningText = "", JReadOptions? options = null)
```

#### Simple Input
```csharp
using JRead;

// Basic input
string name = JRead.Read();
Console.WriteLine($"Hello, {name}!");

// Input with prompt
string input = JRead.Read(beginningText: "Enter your name: ");
Console.WriteLine($"You entered: {input}");

// Input with prefilled text
string edited = JRead.Read("DefaultValue", "Edit this: ");
Console.WriteLine($"Final value: {edited}");
```

### Advanced Usage with JReadOptions

#### Password Input (Masked)

**EnableMaskedInput**: When set to `true`, replaces all input characters with a mask character for security.  
**MaskedInputChar**: The character used to mask input (default: `*`).  
**AddReturnedValueToHistory**: Controls whether the input is saved to command history (should be `false` for passwords).

```csharp
var options = new JReadOptions
{
    EnableMaskedInput = true,
    MaskedInputChar = '*',
    AddReturnedValueToHistory = false // Don't save passwords to history
};

string password = JRead.Read(beginningText: "Password: ", options: options);
```

#### Autocomplete

**EnableAutoComplete**: Enables tab-completion functionality.  
**AutoCompleteItems**: List of strings available for autocompletion.  
**AutoCompleteMinLength**: Minimum number of characters typed before showing suggestions.  
**AutoCompleteCaseSensitive**: Whether autocomplete matching is case-sensitive.

```csharp
var options = new JReadOptions
{
    EnableAutoComplete = true,
    AutoCompleteItems = new List<string> { "apple", "banana", "cherry", "date" },
    AutoCompleteMinLength = 2,
    AutoCompleteCaseSensitive = false
};

string fruit = JRead.Read(beginningText: "Choose a fruit: ", options: options);
// Type "ap" and press Tab to complete to "apple"
```

#### Custom History

**CustomHistory**: Provides a custom JReadHistory instance instead of using the global history.  
**AddReturnedValueToHistory**: When `true`, adds the final input to the history for future sessions.

```csharp
var customHistory = new JReadHistory();
customHistory.Add("previous command 1");
customHistory.Add("previous command 2");

var options = new JReadOptions
{
    CustomHistory = customHistory,
    AddReturnedValueToHistory = true
};

string command = JRead.Read(beginningText: "Command: ", options: options);
// Use Up/Down arrows to navigate through history
```

#### Nullable Input (Can Return Null on Escape)

**EscapingReturnsTheOriginalInput**: When `false`, pressing Escape returns `null`. When `true` (default), returns the original prefilled text.

```csharp
var options = new JReadOptions
{
    EscapingReturnsTheOriginalInput = false // Escape returns null instead of original
};

string? input = JRead.ReadNull(beginningText: "Optional input (ESC to cancel): ", options: options);
if (input == null)
{
    Console.WriteLine("Input cancelled!");
}
```

#### Debug Mode

**EnableDebug**: When `true`, displays debugging information about key presses and internal state.

```csharp
var options = new JReadOptions
{
    EnableDebug = true // Shows key press information
};

string input = JRead.Read(beginningText: "Debug mode: ", options: options);
```

### Keyboard Shortcuts

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

### ✅ Implemented

- **Line Editing:** Insert, delete, overwrite text anywhere in the input line
- **Cursor Movement:** Move cursor by character, word, or to start/end of line
- **Autocompletion:** Tab-complete words with customizable suggestions
- **Undo/Redo:** Full undo/redo support with Ctrl+U/Z and Ctrl+Y
- **Masked Input:** Hide input for passwords or sensitive data
- **Command History:** Navigate through previous inputs with Up/Down arrows
- **Terminal Resize Support:** Handles terminal resizing gracefully
- **Cross-Platform:** Works on Windows, Linux, and macOS
- **Customizable Options:** Extensive configuration through JReadOptions

### 🚧 Planned Features

- **Input Validation:** Validate input in real-time or before submission
- **Syntax Highlighting:** Colorize input for commands, expressions, etc.
- **Multi-line Input:** Edit and submit multi-line text
- **Text Selection:** Keyboard and mouse support for selecting text
- **Customizable Prompts:** Change prompt text, colors, and styles
- **ANSI/VT100 Support:** Colors, formatting, and effects
- **Mouse Support:** Click for cursor movement and selection
- **Async/Task Support:** Non-blocking input for async applications
- **Custom Key Bindings:** Remap shortcuts for your workflow
- **Extensible:** Plugins for completion, highlighting, and more