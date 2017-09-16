var page = require('webpage').create();
var system = require('system');
var url = system.args[1];
var selector = system.args[2];
var timeout = system.args[3] || 5 * 60 * 1000;

page.settings.resourceTimeout = timeout;
var timeout = setTimeout(function () {
    page.stop();
}, timeout);

var started = Date.now();

page.open(url, function (status) {
    var text = null;
    if (status === 'success') {
        text = page.evaluate(function (selector) {
            var element = document.querySelector(selector);
            if (element === null) return null;
            return element.outerHTML;
        }, selector);
    }
    var completed = Date.now();
    console.log(
        JSON.stringify({
            url: url,
            selector: selector,
            status: status,
            text: text,
            startedAt: "\/Date(" + started + ")\/",
            completedAt: "\/Date(" + completed + ")\/",
        })
    );
    phantom.exit();
});
