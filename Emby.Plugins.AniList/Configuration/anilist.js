﻿define(['baseView', 'loading', 'emby-input', 'emby-button', 'emby-checkbox', 'emby-scroller'], function(BaseView, loading) {
    'use strict';

    function loadPage(page, config)
    {

        page.querySelector('#downloadCharacters').checked = config.ShouldDownloadCharacters || '';

        loading.hide();
    }

    function onSubmit(e)
    {

        e.preventDefault();

        loading.show();

        var form = this;

        ApiClient.getNamedConfiguration("anilist").then(function(config) {

            config.ShouldDownloadCharacters = form.querySelector('#downloadCharacters').checked;

            ApiClient.updateNamedConfiguration("anilist", config).then(Dashboard.processServerConfigurationUpdateResult);
        });

    // Disable default form submission
    return false;
}

function getConfig()
{

    return ApiClient.getNamedConfiguration("anilist");
}

function View(view, params)
{
    BaseView.apply(this, arguments);

    view.querySelector('form').addEventListener('submit', onSubmit);
}

Object.assign(View.prototype, BaseView.prototype);

View.prototype.onResume = function(options) {

    BaseView.prototype.onResume.apply(this, arguments);

    loading.show();

    var page = this.view;

    getConfig().then(function(response) {

        loadPage(page, response);
    });
};

return View;

});