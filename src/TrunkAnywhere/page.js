// Trunk Anywhere's web half. Runs in Root.html, the document every game page iframe sits in. On the bag screen's
// loot view it adds a Take All to Trunk button under the game's Take by Type button, styled as the game's own. The
// button only asks; the plugin reads the screen again and makes the game's own whole-container move into the trunk.
// Vue redraws the buttons' column whenever the screen changes, so it is checked on a short poll. Sent again by the
// plugin every couple of seconds; the guard below makes a second copy return at once.
(function () {
  if (window.__trunkanywhere) return;

  window.__trunkanywhere = true;

  var PREFIX = 'slmod|trunkanywhere|';
  var POLL_MS = 400;
  var BUTTON_CLASS = 'ta-take-trunk';
  var DEFAULT_LABEL = 'Take All to Trunk';
  var ARROW = '\u25C0';

  var reported = {};

  function send(message) {
    try {
      if (window.vuplex) window.vuplex.postMessage(PREFIX + message);
    } catch (e) {}
  }

  // One note per kind of failure, so a page a game update reshaped says so once instead of every poll.
  function diag(kind, error) {
    if (reported[kind]) return;

    reported[kind] = true;
    send('diag|' + kind + ': ' + error);
  }

  // ===== What the screen shows =====

  // Reads the page's Vue state, which a game update can rename. A state that cannot be read hides the button: the
  // game's own buttons still work, and a button shown on the wrong screen would only be refused by the plugin.
  function buttonState(doc) {
    try {
      var setup = doc.getElementById('app')._vnode.component.setupState;
      var tabs = setup.aTabs || [];
      var trunkTab = -1;

      for (var i = 0; i < tabs.length; i++) {
        if (tabs[i] && tabs[i].isTrunkTab === true) trunkTab = i;
      }

      var isWanted = setup.isDualBag === true && setup.isTrunk !== true && trunkTab >= 0 && tabs[trunkTab].readonly !== true && setup.curATab !== trunkTab;

      return { isWanted: isWanted, isEmpty: setup.toSelfEmpty === true };
    } catch (e) {
      diag('cannot read the bag screen state, the Take All to Trunk button stays hidden', e);

      return { isWanted: false, isEmpty: true };
    }
  }

  // ===== The button =====

  // The plugin sets the label in the game's language on every send, so it follows a language change.
  function buttonLabel() {
    return window.__trunkanywhereLabel || DEFAULT_LABEL;
  }

  function makeButton(doc) {
    var button = doc.createElement('button');
    var arrow = doc.createElement('span');
    var label = doc.createElement('span');

    button.className = 'transfer-btn btn-dir-left ' + BUTTON_CLASS;
    arrow.className = 'btn-arrow';
    arrow.textContent = ARROW;
    label.className = 'btn-label';
    label.textContent = buttonLabel();
    button.appendChild(arrow);
    button.appendChild(label);
    button.addEventListener('click', function () { send('takeAllToTrunk'); });

    return button;
  }

  // Right under the game's Take by Type button, the first dropdown in the column; before the gap above the Leave
  // buttons if a game update drops that dropdown.
  function placeButton(controls, button) {
    var takeByType = controls.querySelector('.category-dropdown-wrapper');
    var spacer = controls.querySelector('.transfer-controls-spacer');

    if (takeByType && takeByType.parentNode === controls) controls.insertBefore(button, takeByType.nextSibling);
    else if (spacer && spacer.parentNode === controls) controls.insertBefore(button, spacer);
    else controls.appendChild(button);
  }

  function patchControls(doc) {
    var controls = doc.querySelector('.transfer-controls');

    if (!controls) return;

    var state = buttonState(doc);
    var button = controls.querySelector('.' + BUTTON_CLASS);

    if (!button) {
      if (!state.isWanted) return;

      button = makeButton(doc);
      placeButton(controls, button);
    }

    var label = button.querySelector('.btn-label');

    if (label && label.textContent !== buttonLabel()) label.textContent = buttonLabel();

    button.style.display = state.isWanted ? '' : 'none';
    button.disabled = state.isEmpty;
  }

  // ===== Finding the page =====

  // The game's pages are same-origin iframes of this document, created and dropped whenever the game likes.
  function patchPages() {
    var frames = document.querySelectorAll('iframe');

    for (var i = 0; i < frames.length; i++) {
      var src = frames[i].src || '';

      if (!/BackpackUI\.html/.test(src)) continue;

      var doc = null;

      try {
        doc = frames[i].contentDocument;
      } catch (e) {}

      if (!doc) {
        diag('page access', 'cannot reach the bag screen document (' + src + ')');
        continue;
      }

      if (doc.readyState === 'loading') continue;

      try {
        patchControls(doc);
      } catch (e) {
        diag('bag screen', e);
      }
    }
  }

  setInterval(function () {
    try {
      patchPages();
    } catch (e) {
      diag('poll', e);
    }
  }, POLL_MS);
})();
