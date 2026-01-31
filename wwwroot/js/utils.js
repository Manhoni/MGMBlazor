window.abrirPdfNoNavegador = (base64String) => {
      try {
            const cleanBase64 = base64String.replace(/\s/g, '');

            const byteCharacters = atob(cleanBase64);
            const byteNumbers = new Array(byteCharacters.length);
            for (let i = 0; i < byteCharacters.length; i++) {
                  byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);
            const blob = new Blob([byteArray], { type: 'application/pdf' });
            const fileURL = URL.createObjectURL(blob);
            window.open(fileURL, '_blank');
      } catch (e) {
            console.error("Erro ao converter PDF: O Base64 enviado é inválido ou está incompleto (Comum em Sandbox).", e);
            alert("Não foi possível abrir o PDF. O banco enviou um arquivo corrompido (comum em ambiente de teste).");
      }
};