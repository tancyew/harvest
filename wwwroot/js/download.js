// Helper function to download files from base64 data
window.downloadFile = (filename, base64Data) => {
    const link = document.createElement('a');
    link.download = filename;
    link.href = 'data:application/pdf;base64,' + base64Data;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
