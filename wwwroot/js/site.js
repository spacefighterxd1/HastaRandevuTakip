// Site.js - Genel JavaScript fonksiyonları

// Sayfa yüklendiğinde
document.addEventListener('DOMContentLoaded', function() {
    // Alert'leri otomatik kapat (5 saniye sonra)
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Form validasyonu
    const forms = document.querySelectorAll('.needs-validation');
    Array.from(forms).forEach(function(form) {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });

    // TC Kimlik No formatı
    const tcInputs = document.querySelectorAll('input[name="TCKimlikNo"]');
    tcInputs.forEach(function(input) {
        input.addEventListener('input', function(e) {
            this.value = this.value.replace(/[^0-9]/g, '');
            if (this.value.length > 11) {
                this.value = this.value.slice(0, 11);
            }
        });
    });

    // Telefon formatı
    const phoneInputs = document.querySelectorAll('input[type="tel"]');
    phoneInputs.forEach(function(input) {
        input.addEventListener('input', function(e) {
            this.value = this.value.replace(/[^0-9+\-() ]/g, '');
        });
    });
});

// Silme işlemi onayı
function confirmDelete(message) {
    return confirm(message || 'Bu kaydı silmek istediğinizden emin misiniz?');
}

// Tarih formatı
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('tr-TR', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}


