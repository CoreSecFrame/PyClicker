// Aparición de secciones al hacer scroll
const sections = document.querySelectorAll('section');
const showOnScroll = () => {
  const triggerBottom = window.innerHeight * 0.85;
  sections.forEach(sec => {
    const boxTop = sec.getBoundingClientRect().top;
    if (boxTop < triggerBottom) {
      sec.classList.add('visible');
    }
  });
};
window.addEventListener('scroll', showOnScroll);
window.addEventListener('load', showOnScroll);

// Smooth scroll para navegación
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

// Cerrar dropdown si se hace click fuera
window.addEventListener('click', (e) => {
  if (!e.target.matches('.dropdown-btn')) {
    document.querySelectorAll('.dropdown').forEach(drop => {
      drop.classList.remove('show');
    });
  }
});
