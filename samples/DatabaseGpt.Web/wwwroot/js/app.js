function uuid() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
}

async function ask(conversationId, message) {
    const request = { conversationId: conversationId, message: message }
    const response = await fetch('/api/chat/ask', {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(request)
    });

    return response;
}

function GetErrorMessage(statusCode, content)
{
    if (statusCode >= 200 && statusCode <= 299)
        return null;

    if (content.errors)
    {
        return `${content.title ?? content} (${content.errors[0].message})`;
    }

    return content.detail ?? content.title ?? content;
}

function sleep(time) {
    return new Promise((resolve) => {
        setTimeout(resolve, time);
    });
}

async function copyToClipboard(element, text)
{
    let tooltip = bootstrap.Tooltip.getInstance(element);
    tooltip.hide();

    navigator.clipboard.writeText(text);

    element.setAttribute('data-bs-title', 'Copied!');

    tooltip = new bootstrap.Tooltip(element);
    tooltip.show();

    await sleep(3000);
    tooltip.hide();

    // Resets the tooltip title
    element.setAttribute('data-bs-title', 'Copy to clipboard');
    new bootstrap.Tooltip(element);
}