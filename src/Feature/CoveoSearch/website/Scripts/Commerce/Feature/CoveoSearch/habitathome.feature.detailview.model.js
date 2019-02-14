var DetailViewViewModel = function () {
    var detailViewViewModelSelf = this;

    detailViewViewModelSelf.productId = ko.observable();
    detailViewViewModelSelf.name = ko.observable();
    detailViewViewModelSelf.price = ko.observable();
    detailViewViewModelSelf.discountedPrice = ko.observable();
    detailViewViewModelSelf.categories = ko.observable();
    detailViewViewModelSelf.reportingCategory = ko.observable();
    detailViewViewModelSelf.brands = ko.observable();
    detailViewViewModelSelf.reportingBrand = ko.observable();
    detailViewViewModelSelf.taxonomy = ko.observable();
    detailViewViewModelSelf.language = ko.observable();

    detailViewViewModelSelf.IsValid = function () {
        return detailViewViewModelSelf.productId() &&
               detailViewViewModelSelf.name() &&
               detailViewViewModelSelf.price();
    };
}