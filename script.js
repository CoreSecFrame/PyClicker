// Show sections on scroll
const sections = document.querySelectorAll('section');
const showOnScroll = () => {
  const triggerBottom = window.innerHeight * 0.85;
  sections.forEach(sec => {
    const boxTop = sec.getBoundingClientRect().top;
    if (boxTop < triggerBottom) {
      sec.classList.add('visible');

      // Staggered animation for cards
      const cards = sec.querySelectorAll('.card, .testimonial-card, .pricing-card');
      cards.forEach((card, index) => {
        setTimeout(() => {
          card.style.opacity = '1';
          card.style.transform = 'translateY(0)';
        }, index * 150);
      });
    }
  });
};
window.addEventListener('scroll', showOnScroll);
window.addEventListener('load', showOnScroll);

// Smooth scroll for navigation
document.querySelectorAll('.nav-links a').forEach(anchor => {
  anchor.addEventListener('click', e => {
    e.preventDefault();
    const target = document.querySelector(anchor.getAttribute('href'));
    target.scrollIntoView({ behavior: 'smooth' });
  });
});

// Dropdown toggle
document.querySelectorAll('.dropdown-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    btn.parentElement.classList.toggle('show');
  });
});

// Close dropdown when clicking outside
window.addEventListener('click', (e) => {
  if (!e.target.matches('.dropdown-btn')) {
    document.querySelectorAll('.dropdown').forEach(drop => {
      drop.classList.remove('show');
    });
  }
});

// Collapsible section
document.querySelectorAll('.collapsible-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    const content = btn.closest('.pro-features').querySelector('.collapsible-content');
    if (content.style.display === 'block') {
      content.style.display = 'none';
      btn.innerHTML = 'View Details ▾';
    } else {
      content.style.display = 'block';
      btn.innerHTML = 'Hide Details ▴';
    }
  });
});

document.addEventListener("DOMContentLoaded", () => {
  // Abrir overlays genéricos por data-overlay
  document.querySelectorAll('[data-overlay]').forEach(link => {
    link.addEventListener('click', (e) => {
      e.preventDefault();
      const overlayId = link.getAttribute('data-overlay');
      const overlay = document.getElementById(overlayId);
      if (overlay) overlay.classList.add('show');
    });
  });

  // Abrir overlay de compra con botón específico
  const buyBtn = document.getElementById("buyBtn");
  const buyOverlay = document.getElementById("buyOverlay");
  if (buyBtn && buyOverlay) {
    buyBtn.addEventListener("click", () => buyOverlay.classList.add("show"));
  }

  // Cerrar overlays al hacer clic en botón de cerrar o en el fondo
  document.querySelectorAll('.overlay').forEach(overlay => {
    // Clic en fondo
    overlay.addEventListener('click', (e) => {
      if (e.target === overlay) overlay.classList.remove('show');
    });

    // Clic en botón de cerrar
    const closeBtn = overlay.querySelector('.close-btn');
    if (closeBtn) {
      closeBtn.addEventListener('click', (e) => {
        e.preventDefault();
        overlay.classList.remove('show');
      });
    }
  });

  // Agregar ID único al campo custom de PayPal en overlay de compra
  const paypalForm = document.getElementById("paypalOverlayForm");
  if (paypalForm) {
    paypalForm.addEventListener("submit", () => {
      const email = document.getElementById("overlayEmail").value.trim();
      const key = document.getElementById("overlayKey").value.trim();
      const uniqueId = "pyc-" + Date.now();
      document.getElementById("customFieldOverlay").value = `${uniqueId}|${email}|${key}`;
    });
  }
});
