// ==UserScript==
// @name          Spotify Web - Download
// @description   Right click > Download. Requires the protocol handler 'spotdl'.
// @namespace     https://github.com/Silverfeelin/
// @version       0.1
// @license       MIT
// @copyright     2019, Silverfeelin
// @include       https://open.spotify.com/*
// @require       https://code.jquery.com/jquery-3.3.1.min.js
// ==/UserScript==

"use strict";
let $ = window.jQuery;

let pattern = /https:\/\/open\.spotify\.com\/([a-zA-Z]+)\/[a-zA-Z0-9]+/
let allowed = {
    playlist: true,
    album: true,
    track: true
};

/* Finds the Spotify link. Returns { url, type }. */
let findLink = function (target) {
    let url, type;
    target.find('div > textarea').each(function() {
        if (url) return;
        let val = $(this).val();
        let res = pattern.exec(val);
        if (res && res[1] && allowed[res[1]]) {
            url = val;
            type = res[1];
        }
    });
    if (!url) return null;
    return {
        url: url,
        type: type
    };
}

let run = function(link) {
    if (!link || !link.url || !link.type) console.error("Can't call spotify-downloader without a target URL.");
    if ($('#ispotifydl').length == 0) {
        $('<iframe id="ispotifydl" style="display:none" name="ispotifydl"></iframe>').appendTo($('#main'));
    }
    window.open(`spotdl:${link.url}`, 'ispotifydl');
}

/* Creates a Download context menu item and prepends it to target. */
let createEntry = function() {
    let menu = $(".react-contextmenu--visible");
    if (menu.length > 0 && menu.find('.spotdl').length == 0) {
        let link = findLink(menu);
        if (!link) return;
        $('<div class="react-contextmenu-item spotdl" role="menuitem" tabindex="-1" aria-disabled="false">Download</div>').prependTo(menu).click(() => {
            link = findLink(menu);
            if (!link) return;
            run(link);
        });
    }
}

/* Binds createEntry to new items (tracks/albums/etc). */
let addContext = function () {
    let wrapper = $(".react-contextmenu-wrapper:not(.spotdl)");
    wrapper.addClass('spotdl').bind("contextmenu.spotdl", () => window.setTimeout(() => createEntry(), 10));
}

window.setInterval(addContext, 100);
