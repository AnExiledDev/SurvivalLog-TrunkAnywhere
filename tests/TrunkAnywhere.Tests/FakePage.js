// Just enough of Root.html and the bag screen (BackpackUI.html) for page.js to run in Jint: the root document with the
// bag screen's iframe, the vuplex bridge, a DOM with click dispatch, a poll, and the loot view's column of transfer
// buttons with the Vue state the script reads. Test code only; nothing here ships.
var window = this;

// ---------------------------------------------------------------- poll

var intervals = [];

function setInterval(callback) {
  intervals.push(callback);

  return intervals.length;
}

// One run of every interval the script set, which is how its poll runs.
function poll() {
  intervals.slice().forEach(function (callback) { callback(); });
}

// ---------------------------------------------------------------- DOM

var FakeElement = class {
  constructor(doc, tagName) {
    var element = this;

    this.ownerDocument = doc;
    this.tagName = String(tagName).toUpperCase();
    this.id = '';
    this.parentElement = null;
    this.children = [];
    this.text = '';
    this.classes = [];
    this.style = {};
    this.listeners = [];
    this.disabled = false;
    this.classList = {
      contains: function (name) { return element.classes.indexOf(name) >= 0; }
    };
  }

  get className() { return this.classes.join(' '); }

  set className(value) { this.classes = String(value).split(' ').filter(function (name) { return name; }); }

  get parentNode() { return this.parentElement; }

  get nextSibling() {
    var siblings = this.parentElement ? this.parentElement.children : [];
    var index = siblings.indexOf(this);

    return index >= 0 && index + 1 < siblings.length ? siblings[index + 1] : null;
  }

  get textContent() { return this.text + this.children.map(function (child) { return child.textContent; }).join(''); }

  set textContent(value) {
    this.children.slice().forEach(function (child) { this.removeChild(child); }, this);
    this.text = value == null ? '' : String(value);
  }

  appendChild(child) {
    return this.insertBefore(child, null);
  }

  insertBefore(child, before) {
    if (child.parentElement) child.parentElement.removeChild(child);

    var index = before ? this.children.indexOf(before) : -1;

    child.parentElement = this;

    if (index < 0) this.children.push(child);
    else this.children.splice(index, 0, child);

    return child;
  }

  removeChild(child) {
    this.children = this.children.filter(function (c) { return c !== child; });
    child.parentElement = null;

    return child;
  }

  remove() {
    if (this.parentElement) this.parentElement.removeChild(this);
  }

  matches(selector) {
    var parts = parseSelector(selector);

    if (parts.tag && parts.tag !== this.tagName) return false;

    if (parts.id && parts.id !== this.id) return false;

    var element = this;

    return parts.classes.every(function (name) { return element.classList.contains(name); });
  }

  querySelectorAll(selector) {
    return descendantsOf(this).filter(function (element) { return element.matches(selector); });
  }

  querySelector(selector) {
    return this.querySelectorAll(selector)[0] || null;
  }

  addEventListener(type, listener) {
    this.listeners.push({ type: type, listener: listener });
  }
};

// Compound selectors of one tag, one id and classes ("iframe", "#app", ".a.b"); no combinators.
function parseSelector(selector) {
  var match = /^([a-zA-Z][a-zA-Z0-9]*)?((?:[.#][\w-]+)*)$/.exec(selector);

  if (!match) throw new Error('the fake page does not know the selector "' + selector + '"');

  var parts = { tag: match[1] ? match[1].toUpperCase() : '', id: '', classes: [] };

  (match[2].match(/[.#][\w-]+/g) || []).forEach(function (step) {
    if (step.charAt(0) === '#') parts.id = step.slice(1);
    else parts.classes.push(step.slice(1));
  });

  return parts;
}

function descendantsOf(root) {
  var found = [];

  root.children.forEach(function walk(child) {
    found.push(child);
    child.children.forEach(walk);
  });

  return found;
}

var FakeDocument = class {
  constructor() {
    this.readyState = 'complete';
    this.body = new FakeElement(this, 'body');
  }

  createElement(tagName) {
    return new FakeElement(this, tagName);
  }

  getElementById(id) {
    return descendantsOf(this.body).filter(function (element) { return element.id === id; })[0] || null;
  }

  querySelectorAll(selector) {
    return descendantsOf(this.body).filter(function (element) { return element.matches(selector); });
  }

  querySelector(selector) {
    return this.querySelectorAll(selector)[0] || null;
  }
};

// A click on an element and every element above it, as a browser bubbles it. A disabled button gets no click.
function press(target) {
  if (target.disabled) return;

  var event = { type: 'click', target: target };

  for (var element = target; element; element = element.parentElement) {
    element.listeners.slice().forEach(function (entry) {
      if (entry.type === 'click') entry.listener.call(element, event);
    });
  }
}

// Root.html's own document, which holds the page iframes.
var document = new FakeDocument();

// The vuplex bridge: what the script posts to the plugin.
var posted = [];

window.vuplex = {
  postMessage: function (message) { posted.push(message); }
};

// ---------------------------------------------------------------- the bag screen

function node(doc, parent, tagName, className) {
  var element = parent.appendChild(doc.createElement(tagName));

  if (className) element.className = className;

  return element;
}

// The page's setup values the script reads, as Vue's setupState hands them out (refs unwrapped). The default is
// the loot view at home: the backpack tab shown, the car's trunk as the second tab, a crate with items on the right.
function lootState() {
  return {
    isDualBag: true,
    isTrunk: false,
    aTabs: [
      { name: 'Backpack', readonly: false, isTrunkTab: false },
      { name: 'Trunk', readonly: false, isTrunkTab: true }
    ],
    curATab: 0,
    toSelfEmpty: false
  };
}

// The column of transfer buttons between the two sides (BackpackUI.html:3632), as Vue draws it for the loot view:
// Take All, Take by Type, the gap, Leave All, Leave by Type.
function drawControls(doc) {
  var container = doc.querySelector('.backpack-container');
  var controls = node(doc, container, 'div', 'transfer-controls unity-interactive');

  node(doc, controls, 'button', 'transfer-btn transfer-btn-primary btn-dir-left').textContent = 'Take All';
  node(doc, node(doc, controls, 'div', 'category-dropdown-wrapper'), 'button', 'transfer-btn category-dropdown-btn btn-dir-left').textContent = 'Take by Type';
  node(doc, controls, 'div', 'transfer-controls-spacer');
  node(doc, controls, 'button', 'transfer-btn transfer-btn-primary btn-dir-right').textContent = 'Leave All';
  node(doc, node(doc, controls, 'div', 'category-dropdown-wrapper'), 'button', 'transfer-btn category-dropdown-btn btn-dir-right').textContent = 'Leave by Type';

  return controls;
}

// Opens the bag screen: its page in a fresh iframe, as the game loads it, showing the given state.
function openBag(state) {
  var doc = new FakeDocument();
  var frame = document.body.appendChild(document.createElement('iframe'));
  var app = node(doc, doc.body, 'div');

  app.id = 'app';
  app._vnode = { component: { setupState: state } };
  node(doc, app, 'div', 'backpack-container');
  drawControls(doc);
  frame.src = 'https://game.local/UI/BackpackUI/BackpackUI.html';
  frame.contentDocument = doc;

  return doc;
}

function bagDoc() {
  return document.querySelector('iframe').contentDocument;
}

function bagState() {
  return bagDoc().getElementById('app')._vnode.component.setupState;
}

function controls() {
  return bagDoc().querySelector('.transfer-controls');
}

// Vue dropping the column and drawing it afresh, as it does when the loot view closes and opens again.
function redrawControls() {
  controls().remove();
  drawControls(bagDoc());
}

function trunkButtons() {
  return bagDoc().querySelectorAll('.ta-take-trunk');
}

function trunkButton() {
  return trunkButtons()[0] || null;
}

function isShown(button) {
  return !!button && button.style.display !== 'none';
}

// Where an element sits among its parent's children.
function indexInParent(element) {
  return element.parentElement.children.indexOf(element);
}
