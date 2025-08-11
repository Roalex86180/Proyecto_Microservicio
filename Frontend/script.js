// =====================================================================
// Paso 1: Datos simulados de las certificaciones con la nueva propiedad 'platform'
// =====================================================================
const courses = [
    // Certificaciones de AWS
    {
        id: '1',
        title: 'AWS Certified Cloud Practitioner',
        description: 'Aprende los conceptos fundamentales de la nube de AWS.',
        price: 99.00,
        image: 'aws-certified-cloud-practitioner.jpeg',
        platform: 'aws'
    },
    {
        id: '2',
        title: 'AWS Certified Solutions Architect – Associate',
        description: 'Diseña arquitecturas escalables y de alta disponibilidad en AWS.',
        price: 150.00,
        image: 'aws-certified-solutions-architect-associate.jpeg',
        platform: 'aws'
    },
    {
        id: '3',
        title: 'AWS Certified Developer – Associate',
        description: 'Desarrolla aplicaciones con servicios de AWS como Lambda, DynamoDB y SQS.',
        price: 150.00,
        image: 'aws-certified-developer-associate.jpeg',
        platform: 'aws'
    },
    {
        id: '4',
        title: 'AWS Certified SysOps Administrator – Associate',
        description: 'Opera sistemas en AWS, gestionando despliegues y automatizando procesos.',
        price: 150.00,
        image: 'aws-certified-sysops-administrator-associate.jpeg',
        platform: 'aws'
    },
    {
        id: '5',
        title: 'AWS Certified DevOps Engineer – Professional',
        description: 'Implementa y gestiona sistemas de entrega continua en AWS.',
        price: 300.00,
        image: 'aws-devops-engineer-professional.jpeg', // <--- Revisa este nombre
        platform: 'aws'
    },
    // Certificaciones de Azure
    {
        id: '6',
        title: 'Microsoft Certified: Azure Fundamentals (AZ-900)',
        description: 'Aprende los conceptos básicos de los servicios en la nube de Azure.',
        price: 99.00,
        image: 'microsoft-certified-azure-fundamentals.jpeg',
        platform: 'azure'
    },
    {
        id: '7',
        title: 'Microsoft Certified: Azure Administrator Associate (AZ-104)',
        description: 'Administra y opera entornos de Azure, incluyendo redes y seguridad.',
        price: 165.00,
        image: 'microsoft-certified-azure-administrator-associate.jpeg',
        platform: 'azure'
    },
    {
        id: '8',
        title: 'Microsoft Certified: Azure Developer Associate (AZ-204)',
        description: 'Desarrolla soluciones de Azure con funciones, contenedores y bases de datos.',
        price: 165.00,
        image: 'microsoft-certified-azure-developer-associate.jpeg', // <--- Revisa este nombre
        platform: 'azure'
    },
    {
        id: '9',
        title: 'Microsoft Certified: Azure Security Engineer Associate (AZ-500)',
        description: 'Protege entornos de Azure, gestionando identidades y accesos.',
        price: 165.00,
        image: 'microsoft-certified-azure-security-engineer-associate.jpeg', // <--- Revisa este nombre
        platform: 'azure'
    },
    {
        id: '10',
        title: 'Microsoft Certified: Azure AI Engineer Associate (AI-102)',
        description: 'Crea soluciones de IA con servicios cognitivos y machine learning de Azure.',
        price: 165.00,
        image: 'microsoft-certified-azure-ai-engineer-associate.jpeg', // <--- Revisa este nombre
        platform: 'azure'
    },
    // Certificaciones de Google Cloud
    {
        id: '11',
        title: 'Google Cloud Certified - Cloud Digital Leader',
        description: 'Conoce los fundamentos de Google Cloud y el impacto en el negocio.',
        price: 99.00,
        image: 'google-cloud-certified-cloud-digital-leader.jpeg',
        platform: 'google'
    },
    {
        id: '12',
        title: 'Google Cloud Certified - Associate Cloud Engineer',
        description: 'Configura y gestiona recursos y servicios en la plataforma de Google Cloud.',
        price: 125.00,
        image: 'google-cloud-certified-associate-cloud-engineer.jpeg',
        platform: 'google'
    },
    {
        id: '13',
        title: 'Google Cloud Certified - Professional Cloud Architect',
        description: 'Diseña, planifica y gestiona arquitecturas de Google Cloud.',
        price: 200.00,
        image: 'google-cloud-certified-professional-cloud-architect.jpeg', // <--- Revisa este nombre
        platform: 'google'
    },
    {
        id: '14',
        title: 'Google Cloud Certified - Professional Data Engineer',
        description: 'Construye sistemas de procesamiento de datos para soluciones de IA.',
        price: 200.00,
        image: 'google-cloud-certified-professional-data-engineer.jpeg', // <--- Revisa este nombre
        platform: 'google'
    },
    {
        id: '15',
        title: 'Google Cloud Certified - Professional Cloud Security Engineer',
        description: 'Diseña e implementa una infraestructura segura en Google Cloud.',
        price: 200.00,
        image: 'google-cloud-certified-professional-cloud-security-engineer.jpeg', // <--- Revisa este nombre
        platform: 'google'
    }
];
// =====================================================================
// Paso 2: Lógica del Carrito y Eventos
// =====================================================================
let cart = [];

const coursesContainer = document.getElementById('courses-container');
const cartContainer = document.getElementById('cart-items-container');
const cartCountSpan = document.getElementById('cart-count');
const cartTotalSpan = document.getElementById('cart-total');
const cartIcon = document.getElementById('cart-icon');
const cartSection = document.getElementById('cart-section');
const courseCatalogSection = document.getElementById('course-catalog');

function getPlatformIcon(platform) {
    switch(platform) {
        case 'aws':
            return 'aws-icon.png';
        case 'azure':
            return 'azure-icon.png';
        case 'google':
            return 'google-icon.png';
        default:
            return '';
    }
}

// Función para renderizar los cursos en la página
function renderCourses(coursesToRender) {
    coursesContainer.innerHTML = ''; // Limpia el contenedor
    if (coursesToRender.length === 0) {
        coursesContainer.innerHTML = '<p>No se encontraron cursos.</p>';
        return;
    }

    coursesToRender.forEach(course => {
        const courseElement = document.createElement('div');
        courseElement.classList.add('course-card');

        const iconSrc = getPlatformIcon(course.platform);
        const iconHtml = iconSrc ? `<img src="${iconSrc}" alt="Icono de ${course.platform}" class="platform-icon">` : '';

        courseElement.innerHTML = `
            <img src="${course.image}" alt="${course.title}">
            <h3>${iconHtml} ${course.title}</h3>
            <p>${course.description}</p>
            <span>$${course.price.toFixed(2)}</span>
            <button class="add-to-cart-btn" data-id="${course.id}">Añadir al Carrito</button>
        `;
        coursesContainer.appendChild(courseElement);
    });
}

// Función para añadir un curso al carrito
function addToCart(courseId) {
    const course = courses.find(c => c.id === courseId);
    if (course && !cart.find(c => c.id === courseId)) {
        cart.push(course);
        updateCartDisplay();
        alert(`"${course.title}" ha sido añadido al carrito.`);
    } else if (cart.find(c => c.id === courseId)) {
        alert(`"${course.title}" ya está en el carrito.`);
    }
}

// Función para actualizar la visualización del carrito
function updateCartDisplay() {
    cartContainer.innerHTML = '';
    let total = 0;

    if (cart.length === 0) {
        cartContainer.innerHTML = '<p>El carrito está vacío.</p>';
    } else {
        cart.forEach(course => {
            const cartItem = document.createElement('div');
            cartItem.classList.add('cart-item');
            cartItem.innerHTML = `
                <img src="${course.image}" alt="${course.title}">
                <span>${course.title}</span>
                <span>$${course.price.toFixed(2)}</span>
            `;
            cartContainer.appendChild(cartItem);
            total += course.price;
        });
    }

    cartCountSpan.textContent = cart.length;
    cartTotalSpan.textContent = total.toFixed(2);
}

// =====================================================================
// Paso 3: Manejadores de eventos para la interacción del usuario
// =====================================================================
document.getElementById('login-btn').addEventListener('click', () => {
    alert('Simulando inicio de sesión...');
});

// Evento para los botones de "Añadir al Carrito"
coursesContainer.addEventListener('click', (e) => {
    if (e.target.classList.contains('add-to-cart-btn')) {
        const courseId = e.target.dataset.id;
        addToCart(courseId);
    }
});

// Evento para los enlaces de la barra lateral (filtrado)
document.querySelectorAll('.filter-link').forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        const platform = e.target.dataset.platform;
        let filteredCourses = courses;

        if (platform !== 'all') {
            filteredCourses = courses.filter(course => course.platform === platform);
        }
        renderCourses(filteredCourses);
        // Oculta el carrito si está visible
        cartSection.style.display = 'none';
        courseCatalogSection.style.display = 'block';
    });
});

// Evento para el formulario de búsqueda
document.getElementById('search-form').addEventListener('submit', (e) => {
    e.preventDefault();
    const searchTerm = document.getElementById('search-input').value.toLowerCase();
    const filteredCourses = courses.filter(course => 
        course.title.toLowerCase().includes(searchTerm) || 
        course.description.toLowerCase().includes(searchTerm)
    );
    renderCourses(filteredCourses);
});

// Evento para el icono del carrito
cartIcon.addEventListener('click', () => {
    if (cartSection.style.display === 'none') {
        cartSection.style.display = 'block';
        courseCatalogSection.style.display = 'none';
        updateCartDisplay();
    } else {
        cartSection.style.display = 'none';
        courseCatalogSection.style.display = 'block';
    }
});

// Evento para el botón de simular pago
document.getElementById('checkout-btn').addEventListener('click', () => {
    alert(`Simulando pago. Total a pagar: $${cartTotalSpan.textContent}. ¡Gracias por tu compra!`);
    cart = []; // Vaciar el carrito después de la simulación
    updateCartDisplay();
    cartSection.style.display = 'none';
    courseCatalogSection.style.display = 'block';
});


// Lógica para ocultar/mostrar la barra lateral
const sidebarToggleBtn = document.getElementById('sidebar-toggle-btn');
const sidebar = document.querySelector('.sidebar');
const mainContainer = document.querySelector('.main-container');

// La barra lateral y el contenedor principal ya comienzan abiertos gracias al HTML
// Solo necesitamos la lógica para cerrarlos
sidebarToggleBtn.addEventListener('click', () => {
    sidebar.classList.toggle('open');
    mainContainer.classList.toggle('sidebar-open');
    sidebarToggleBtn.classList.toggle('rotated');
});
// =====================================================================
// Inicializar la página
// =====================================================================
document.addEventListener('DOMContentLoaded', () => {
    renderCourses(courses);
    updateCartDisplay();
});