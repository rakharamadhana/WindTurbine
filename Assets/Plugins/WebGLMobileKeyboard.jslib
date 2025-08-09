mergeInto(LibraryManager.library, {
  WebGLFocusNumeric: function(goPtr, initialPtr, min, max, step) {
    var go = UTF8ToString(goPtr);
    var initial = UTF8ToString(initialPtr);

    // Reuse or create a hidden input
    var id = 'unity-webgl-hidden-input';
    var el = document.getElementById(id);
    if (!el) {
      el = document.createElement('input');
      el.id = id;
      el.type = 'text';
      el.inputMode = 'decimal';    // mobile numeric keyboard
      el.autocomplete = 'off';
      el.autocapitalize = 'off';
      el.spellcheck = false;

      // Keep it invisible but focusable
      el.style.position = 'fixed';
      el.style.opacity = '0';
      el.style.pointerEvents = 'none';
      el.style.left = '0';
      el.style.bottom = '0';
      el.style.width = '1px';
      el.style.height = '1px';

      document.body.appendChild(el);

      // Sync to Unity on input
      el.addEventListener('input', function() {
        if (window.unityInstance) {
          window.unityInstance.SendMessage(go, 'OnHtmlInputChanged', el.value);
        }
      });

      // Hide keyboard on Enter
      el.addEventListener('keydown', function(e) {
        if (e.key === 'Enter') {
          el.blur();
          if (window.unityInstance) {
            window.unityInstance.SendMessage(go, 'OnHtmlInputSubmit', el.value);
          }
        }
      });
    }

    // Set starting value
    el.value = initial || '';

    // Focus within a user gesture to guarantee keyboard
    setTimeout(function() { el.focus(); el.select && el.select(); }, 0);
  },

  WebGLBlurNumeric: function() {
    var el = document.getElementById('unity-webgl-hidden-input');
    if (el) el.blur();
  }
});
