(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        // use AMD define funtion to support AMD modules if in use
        define('CXA/Feature/DetailView', ['exports'], factory);
    } else if (typeof exports === 'object') {
        // to support CommonJS
        factory(exports);
    }

    // browser global variable
    root.DetailView = factory;
    root.DetailView_ComponentClass = "cxa-coveosearch-detailview-component";
}(this, function (element) {
    'use strict';
    var component = new Component(element);

    component.GetHiddenInputValue = function (inputName) {
        return $(component.RootElement).find("[name = " + inputName + "]").val()
    };

    // Read values from hidden input fields
    var productId = component.GetHiddenInputValue("currentCatalogItemId");
    var name = component.GetHiddenInputValue("name");
    var price = parseFloat(component.GetHiddenInputValue("price"));
    var discountedPrice = parseFloat(component.GetHiddenInputValue("discountedPrice"));
    var categories = JSON.parse(component.GetHiddenInputValue("categories"));
    var reportingCategory = component.GetHiddenInputValue("reportingCategory");
    var brands = JSON.parse(component.GetHiddenInputValue("brands"));
    var reportingBrand = component.GetHiddenInputValue("reportingBrand");
    var language= component.GetHiddenInputValue("language");

    // Set the model properties
    component.model = new DetailViewViewModel();
    component.model.productId(productId);
    component.model.name(name);
    component.model.price(price);
    component.model.discountedPrice(discountedPrice);
    component.model.categories(categories);
    component.model.reportingCategory(reportingCategory);
    component.model.brands(brands);
    component.model.reportingBrand(reportingBrand);
    component.model.language(language);

    component.Name = "CXA/Feature/DetailView";

    component.Init = function () {
        ko.applyBindings(component.model, component.RootElement);
        component.TrySendDetailViewEvent();
    };

    component.InExperienceEditorMode = function () {
    };

    component.TrySendDetailViewEvent = function () {
        if (component.model.IsValid() && typeof coveoanalytics !== 'undefined') {
            component.SendDetailViewEvent();
        }
    };

    component.SendDetailViewEvent = function () {
        var customData = {
            "contentIDKey": "@z95xname",
            "contentIDValue": component.model.productId(),
            "actionCause": "view",
            "name": component.model.name(),
            "price": component.model.price(),
        };
        if (component.model.price() !== component.model.discountedPrice()) {
            customData.discountedPrice = component.model.discountedPrice();
        }
        if (component.model.categories()) {
            customData.categories = component.model.categories();
        }
        if (component.model.reportingCategory()) {
            customData.reportingCategory = component.model.reportingCategory();
        }
        if (component.model.brands()) {
            customData.brands = component.model.brands();
        }
        if (component.model.reportingBrand()) {
            customData.reportingBrand = component.model.reportingBrand();
        }
        if (component.model.taxonomy()) {
            customData.taxonomy = component.model.taxonomy();
        }

        var coveoAnalyticsClient = new coveoanalytics.analytics.Client({
            endpoint: document.location.origin + '/coveo/rest/coveoanalytics'
        });
        coveoAnalyticsClient.sendCustomEvent({
            eventType: "detailView",
            eventValue: component.model.productId(),
            language: component.model.language(),
            originLevel1: 'shop',
            originLevel2: 'Default',
            originLevel3: document.referrer,
            customData: customData
        });
    };

    return component;
}));
