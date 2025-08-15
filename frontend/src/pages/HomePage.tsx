import { useEffect, useState } from 'react';
import { type Course } from '../types/Course';
import { getCourses, addCourseToCart } from '../api/api';
import Sidebar from '../components/Sidebar';
import SearchBar from '../components/SearchBar';
import CourseActionModal from '../components/CourseActionModal';
import './HomePage.css';

// Importar las imágenes de los iconos
import awsIcon from '../assets/images/aws-icon.png';
import azureIcon from '../assets/images/azure-icon.png';
import googleIcon from '../assets/images/google-icon.png';

// [MODIFICADO] La prop ahora recibe el objeto Course
interface HomePageProps {
  onItemAddedToCart: (course: Course) => void;
}

const HomePage: React.FC<HomePageProps> = ({ onItemAddedToCart }) => {
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isSidebarVisible, setIsSidebarVisible] = useState<boolean>(true);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [selectedCourse, setSelectedCourse] = useState<Course | null>(null);

  const getPlatformIcon = (platform: string) => {
    const platformIcons = {
      aws: awsIcon,
      azure: azureIcon,
      google: googleIcon,
      gcp: googleIcon,
    };
    
    return platformIcons[platform.toLowerCase() as keyof typeof platformIcons] || awsIcon;
  };

  useEffect(() => {
    const fetchCourses = async () => {
      try {
        const coursesData = await getCourses();
        setCourses(coursesData);
      } catch (err) {
        setError('No se pudieron cargar los cursos. Por favor, revisa la consola para más detalles.');
        console.error('Error fetching courses:', err);
      } finally {
        setLoading(false);
      }
    };
    fetchCourses();
  }, []);

  const toggleSidebar = () => {
    setIsSidebarVisible(!isSidebarVisible);
  };

  const handleCourseClick = (course: Course) => {
    setSelectedCourse(course);
  };

  const handleCloseModal = () => {
    setSelectedCourse(null);
  };

  const addToCart = async (courseId: string) => {
    const userId = '12345';
    try {
      await addCourseToCart(userId, courseId);
      
      // [MODIFICADO] Ahora pasamos el objeto 'selectedCourse'
      if (selectedCourse) {
        onItemAddedToCart(selectedCourse);
      }

      alert('¡Curso añadido al carrito!');
    } catch (err: any) {
      if (err.status === 409) {
        alert('¡Este curso ya se encuentra en tu carrito!');
      } else {
        alert('Error al añadir el curso al carrito.');
        console.error(err);
      }
    }
  };

  const filteredCourses = courses.filter(course =>
    course.Name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    course.Description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="home-page-layout">
      <Sidebar isSidebarVisible={isSidebarVisible} toggleSidebar={toggleSidebar} />
      <div className="course-content">
        <SearchBar searchQuery={searchQuery} setSearchQuery={setSearchQuery} />
        {loading && <div>Cargando cursos...</div>}
        {error && <div>{error}</div>}
        {!loading && !error && (
          <div className="course-list">
            {filteredCourses.length > 0 ? (
              filteredCourses.map((course) => (
                <div 
                  key={course.Id} 
                  className="course-card"
                  onClick={() => handleCourseClick(course)}
                >
                  <img 
                    src={getPlatformIcon(course.Platform)} 
                    alt={`${course.Platform} icon`}
                    className="course-icon"
                  />
                  <h3>{course.Name}</h3>
                  <p>{course.Description}</p>
                  <p>Precio: ${course.Price}</p>
                </div>
              ))
            ) : (
              <div>No se encontraron cursos que coincidan con su búsqueda.</div>
            )}
          </div>
        )}
      </div>
      {selectedCourse && (
        <CourseActionModal
          course={selectedCourse}
          onClose={handleCloseModal}
          onAddToCart={() => addToCart(selectedCourse.Id)} 
        />
      )}
    </div>
  );
};

export default HomePage;