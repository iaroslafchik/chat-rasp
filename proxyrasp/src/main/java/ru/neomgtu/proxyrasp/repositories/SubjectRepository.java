package ru.neomgtu.proxyrasp.repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import ru.neomgtu.proxyrasp.entity.Subject;

@Repository
public interface SubjectRepository extends JpaRepository<Subject, Long> {

	boolean existsByAuditoriumAndDateAndBeginLesson(String auditorium, String date, String beginLesson);

}
