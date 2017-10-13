// 引数の設定
var system = require('system');
var url = system.args[1];
var selector = system.args[2];
var expiration = system.args[3] || 5 * 60 * 1000;

// ページの準備
var page = require('webpage').create();
page.settings.resourceTimeout = expiration;

// タイムアウト処理（https://qiita.com/nao58/items/62fe1d9408c52335cfbd）
var timeout = setTimeout(function () {
    page.stop();
    console.log(
        JSON.stringify({
            target: url,
            selector: selector,
            status: "timeout",
            text: "",
        })
    );
    phantom.exit();
}, timeout);

// 読み込みと評価
page.open(url, function (status) {
    window.clearTimeout(timeout);

    var text = null;
    if (status === 'success') {
        text = page.evaluate(function (selector) {
            try {
                var element = document.querySelector(selector);
                if (element !== null) return element.outerHTML;
                
                console.log(
                    JSON.stringify({
                        target: url,
                        selector: selector,
                        status: "Empty",
                        text: "",
                    })
                );
                phantom.exit();

            } catch (e) {
                console.log(
                    JSON.stringify({
                        target: url,
                        selector: selector,
                        status: "Invalid Query",
                        text: "",
                    })
                );
                phantom.exit();
            }
        }, selector);
    }

    console.log(
        JSON.stringify({
            target: url,
            selector: selector,
            status: status,
            text: text,
        })
    );
    phantom.exit();
});
