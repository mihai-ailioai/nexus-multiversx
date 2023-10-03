mergeInto(LibraryManager.library, {
    OpenPopup: function (url, callback) {
        var urlString = Pointer_stringify(url);
        var callbackString = Pointer_stringify(callback);

        const currentURL = new URL(window.location.href);
        const callbackUrl = new URL(currentURL.origin + "/" + callbackString);

        const targetURL = new URL(urlString);
        targetURL.searchParams.append('callbackUrl', callbackUrl.href);

        console.log('Full URL: ' + targetURL.href);
        var popup = window.open(targetURL.href, '_blank', "width=1024,height=1024");

        console.log('Popup: ' + popup);
        var checkInterval = setInterval(function () {
            // Check if popup is null
            if (!popup) {
                console.log('Popup is null');
                clearInterval(checkInterval);
                return;
            }

            // Check if popup is closed by user
            if (popup.closed) {
                console.log('Popup is closed');
                clearInterval(checkInterval);
                return;
            }

            // Check the URL of the popup
            try {
                const destinationURL = new URL(popup.location.href);
                console.log(destinationURL.origin + " " + callbackUrl.origin);
                console.log(destinationURL.pathname + " " + callbackUrl.pathname);

                if (destinationURL.origin === callbackUrl.origin && destinationURL.pathname === callbackUrl.pathname) {
                    clearInterval(checkInterval);
                    popup.close();
                    popup = null;

                    const address = destinationURL.searchParams.get('address');
                    console.log(address);
                    // Notify Unity
                    unityInstance.SendMessage('MultiversX', 'OnPopupNavigationComplete', address);
                }
            } catch (e) {
                console.log(e);
                // Exception might be thrown if popup is on a different domain
                // Just continue monitoring until it comes back to your domain
            }
        }, 100);
    }
});